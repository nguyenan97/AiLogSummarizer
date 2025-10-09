using Application.Interfaces;
using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Shared;
using Infrastructure.Providers;
using Infrastructure.Services.LogSources;
using Infrastructure.Utils;

namespace Infrastructure.Services.Summarizer;

public class SummarizerService: ISummarizerService
{
    private readonly SummarizerProvider _summarizerProvider;

    public SummarizerService(SummarizerProvider summarizerProvider)
    {
        _summarizerProvider = summarizerProvider;
    }

    public async Task<SummarizerResponse> ProcessLogsAsync(
        IEnumerable<TraceLog> logs,
        IntentType intent,
        CancellationToken cancellationToken = default)
    {
        var logList = logs.ToList();
        if (!logList.Any())
            return new SummarizerResponse { RawMarkdown = ":warning: No logs provided." };

        // --- Giai đoạn 1: Chuẩn bị và Xử lý Chunk ---
        var chunkProcessor = _summarizerProvider.GetChunkProcessor();
        var chunks = LogChunker.ChunkLogs(logList);
        var errorChunks = chunks.Where(c => c.ContainsError).ToList();

        if (!errorChunks.Any())
            return new SummarizerResponse { RawMarkdown = ":white_check_mark: No errors found in logs." };

        List<SummarizerResponse> partialSummaries = await chunkProcessor.ProcessChunksAsync(
            errorChunks,
            ProcessingStrategy.Batched, // Luôn dùng chiến lược hiệu quả nhất
            cancellationToken
        );

        // --- Giai đoạn 2: Hợp nhất ---
        var mergeProcessor = _summarizerProvider.GetMergeProcessor();
        SummarizerResponse finalResponse = await mergeProcessor.MergeSummariesAsync(
            partialSummaries,
            logList,
            intent,
            cancellationToken
        );

        // --- Giai đoạn 3: Hoàn thiện ---
        finalResponse.DateRange = $"{logList.Min(l => l.Timestamp):u} → {logList.Max(l => l.Timestamp):u}";
        finalResponse.IntentType = intent;
        finalResponse.RawMarkdown = SlackFormatter.Format(finalResponse);

        return finalResponse;
    }
}
