using Application.Interfaces;
using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;

namespace Infrastructure.Services.Summarizer;

public class GeminiSummarizerService : IChunkProcessorService, IMergeProcessorService
{
    // TODO: Implement Google Gemini integration
    // Next step: Add Google AI SDK and Gemini model client
    // Improvement: Use Gemini for cost-effective processing of large log volumes
    public Task<SummarizerResponse> MergeSummariesAsync(IEnumerable<SummarizerResponse> partialSummaries, IEnumerable<TraceLog> originalLogs, IntentType inputType, InputLanguage language = InputLanguage.Vietnamese, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<SummarizerResponse>> ProcessChunksAsync(List<LogChunk> chunks, ProcessingStrategy strategy, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
