using Application.Interfaces;
using Application.Models;
using Domain.Shared;
using Domain.Models;
using Infrastructure.AiFactory;
using Infrastructure.Prompts;
using Infrastructure.Utils;
using System.Text.Json;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using SharpToken;
using Domain.MentionParsing.Models;

namespace Infrastructure.Services.Summarizer
{
    public class AzureSummarizerService : IChunkProcessorService, IMergeProcessorService
    {
        // --- Fields ---
        private readonly AzureOpenAIFactory _clientFactory;
        private readonly string _modelNameForTokenCount;
        private readonly int _modelTokenLimit;
        private readonly int _safeMargin;

        // --- Constructor ---
        /// <summary>
        /// Khởi tạo service với factory và các cấu hình cụ thể cho model sẽ sử dụng.
        /// </summary>
        public AzureSummarizerService(
            AzureOpenAIFactory clientFactory,
            string modelNameForTokenCount,
            int modelTokenLimit,
            int safeMargin)
        {
            _clientFactory = clientFactory;
            _modelNameForTokenCount = modelNameForTokenCount;
            _modelTokenLimit = modelTokenLimit;
            _safeMargin = safeMargin;
        }

        #region Interface Implementations

        /// <summary>
        /// (Từ IChunkProcessorService) Điều phối việc xử lý các chunk log theo chiến lược đã chọn.
        /// </summary>
        public async Task<List<SummarizerResponse>> ProcessChunksAsync(List<LogChunk> chunks, ProcessingStrategy strategy, CancellationToken cancellationToken = default)
        {
            var chatClient = _clientFactory.CreateChatClient();
            return strategy switch
            {
                ProcessingStrategy.Sequential => await ProcessSequentiallyAsync(chatClient, chunks, cancellationToken),
                ProcessingStrategy.Parallel => await ProcessInParallelAsync(chatClient, chunks, cancellationToken),
                ProcessingStrategy.Batched => await ProcessInBatchesAsync(chatClient, chunks, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(strategy), "Unsupported processing strategy."),
            };
        }

        /// <summary>
        /// (Từ IMergeProcessorService) Hợp nhất các kết quả tóm tắt riêng lẻ thành một kết quả cuối cùng.
        /// </summary>
        public async Task<SummarizerResponse> MergeSummariesAsync(IEnumerable<SummarizerResponse> partialSummaries, IEnumerable<TraceLog> originalLogs, IntentType inputType, CancellationToken cancellationToken = default)
        {
            var chatClient = _clientFactory.CreateChatClient();
            var dateRange = $"{originalLogs.Min(l => l.Timestamp):u} → {originalLogs.Max(l => l.Timestamp):u}";

            IEnumerable<ChatMessage> messages = inputType switch
            {
                IntentType.Summarize => SummarizerPromptBuilder.BuildMergePrompt(partialSummaries),
                IntentType.Analyze => SummarizerPromptBuilder.BuildAnalyzePrompt(originalLogs, dateRange),
                IntentType.Report => SummarizerPromptBuilder.BuildReportPrompt(partialSummaries, dateRange),
                _ => throw new ArgumentOutOfRangeException(nameof(inputType))
            };

            var resp = await chatClient.GetResponseAsync<SummarizerResponse>(
                 messages,
                 cancellationToken: cancellationToken
             );

            return resp.Result;
        }

        #endregion

        #region Private Processing Strategy Implementations

        /// <summary>
        /// Chiến lược 1: Xử lý tuần tự từng chunk. Chậm nhưng dễ debug.
        /// </summary>
        private async Task<List<SummarizerResponse>> ProcessSequentiallyAsync(IChatClient chatClient, List<LogChunk> chunks, CancellationToken ct)
        {
            var results = new List<SummarizerResponse>();
            foreach (var chunk in chunks)
            {
                if (ct.IsCancellationRequested) break;
                var summary = await SummarizeSingleChunkAsync(chatClient, chunk, ct);
                if (summary != null) results.Add(summary);
            }
            return results;
        }

        /// <summary>
        /// Chiến lược 2: Xử lý song song nhiều chunk.
        /// </summary>
        private async Task<List<SummarizerResponse>> ProcessInParallelAsync(IChatClient chatClient, List<LogChunk> chunks, CancellationToken ct)
        {
            var results = new ConcurrentBag<SummarizerResponse>();
            const int concurrencyLimit = 10;
            using var semaphore = new SemaphoreSlim(concurrencyLimit);

            var tasks = chunks.Select(async chunk =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    if (ct.IsCancellationRequested) return;
                    var summary = await SummarizeSingleChunkAsync(chatClient, chunk, ct);
                    if (summary != null) results.Add(summary);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            return results.ToList();
        }

        /// <summary>
        /// Chiến lược 3: Xử lý theo lô, gom nhiều chunk vào một request dựa trên giới hạn token từ cấu hình.
        /// </summary>
        private async Task<List<SummarizerResponse>> ProcessInBatchesAsync(IChatClient chatClient, List<LogChunk> chunks, CancellationToken ct)
        {
            var maxTokensPerBatch = _modelTokenLimit - _safeMargin;
            if (maxTokensPerBatch <= 0)
                throw new InvalidOperationException("ModelTokenLimit from configuration must be greater than SafeMargin.");

            var gptEncoding = GptEncoding.GetEncodingForModel(_modelNameForTokenCount);
            var promptTemplateText = SummarizerPromptBuilder.GetBatchChunkPromptTemplate();
            var promptTemplateTokens = gptEncoding.Encode(promptTemplateText).Count;

            var chunkBatches = new List<List<LogChunk>>();
            if (chunks.Any())
            {
                var currentBatch = new List<LogChunk>();
                var currentBatchTokens = promptTemplateTokens;

                foreach (var chunk in chunks)
                {
                    var requestItem = new BatchSummarizeRequestItem { ChunkId = chunk.Fingerprint, LogChunk = string.Join("\n", chunk.Logs.Select(l => l.Message)) };
                    int separatorTokens = currentBatch.Any() ? 1 : 0; // Token cho dấu ','
                    var chunkTokens = gptEncoding.Encode(JsonSerializer.Serialize(requestItem, JsonOptions.Default)).Count + separatorTokens;

                    if (currentBatch.Any() && currentBatchTokens + chunkTokens > maxTokensPerBatch)
                    {
                        chunkBatches.Add(currentBatch);
                        currentBatch = new List<LogChunk> { chunk };
                        currentBatchTokens = promptTemplateTokens + chunkTokens - separatorTokens; // Lô mới không có dấu phẩy ở đầu
                    }
                    else
                    {
                        currentBatch.Add(chunk);
                        currentBatchTokens += chunkTokens;
                    }
                }
                if (currentBatch.Any()) chunkBatches.Add(currentBatch);
            }

            var processingTasks = chunkBatches.Select(batch => ProcessSingleBatchAsync(chatClient, batch, ct));
            var batchResults = await Task.WhenAll(processingTasks);
            return batchResults.SelectMany(r => r).ToList();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gửi một chunk đơn lẻ đến AI để tóm tắt. Dùng cho chiến lược Sequential và Parallel.
        /// </summary>
        private async Task<SummarizerResponse?> SummarizeSingleChunkAsync(IChatClient chatClient, LogChunk chunk, CancellationToken ct)
        {
            try
            {
                var chunkText = string.Join("\n", chunk.Logs.Select(l => l.Message));
                var messages = SummarizerPromptBuilder.BuildChunkPrompt(chunkText);

                var resp = await chatClient.GetResponseAsync<SummarizerResponse>(messages, cancellationToken: ct);
                return resp.Result;
            }
            catch (Exception)
            {
                // Log lỗi ở đây (ví dụ: _logger.LogError(ex, "Failed to summarize single chunk."))
                return null;
            }
        }

        /// <summary>
        /// Gửi một lô chunk đến AI để tóm tắt. Dùng cho chiến lược Batched.
        /// </summary>
        private async Task<List<SummarizerResponse>> ProcessSingleBatchAsync(IChatClient chatClient, List<LogChunk> batch, CancellationToken ct)
        {
            if (ct.IsCancellationRequested || !batch.Any()) return new List<SummarizerResponse>();

            var requestItems = batch.Select(chunk => new BatchSummarizeRequestItem
            {
                ChunkId = chunk.Fingerprint,
                LogChunk = string.Join("\n", chunk.Logs.Select(l => l.Message))
            }).ToList();

            try
            {
                var messages = SummarizerPromptBuilder.BuildBatchChunkPrompt(requestItems);

                var resp = await chatClient.GetResponseAsync<List<BatchSummarizeResponseItem>>(messages, cancellationToken: ct);
                return resp.Result?.Select(r => r.Summary)
                  .OfType<SummarizerResponse>()
                  .ToList() ?? new List<SummarizerResponse>();
            }
            catch (Exception)
            {
                // Log lỗi ở đây (ví dụ: _logger.LogError(ex, "Failed to process batch."))
                return new List<SummarizerResponse>();
            }
        }

        #endregion
    }
}
