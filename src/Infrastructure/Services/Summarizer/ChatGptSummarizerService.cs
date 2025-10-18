using Application.Interfaces;
using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;

namespace Infrastructure.Services.Summarizer
{
    public class ChatGptSummarizerService : IChunkProcessorService, IMergeProcessorService
    {
        // TODO: Implement full ChatGPT integration similar to AzureSummarizerService
        // Next step: Add OpenAI client factory and token management
        // Improvement: Support both GPT-3.5-turbo and GPT-4 models for cost optimization
        public Task<SummarizerResponse> MergeSummariesAsync(IEnumerable<SummarizerResponse> partialSummaries, IEnumerable<TraceLog> originalLogs, IntentType inputType, InputLanguage language = InputLanguage.Vietnamese, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<SummarizerResponse>> ProcessChunksAsync(List<LogChunk> chunks, ProcessingStrategy strategy, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
