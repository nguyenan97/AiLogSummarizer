using Application.Interfaces;
using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;

namespace Infrastructure.Services.Summarizer;

public class GeminiSummarizerService : IChunkProcessorService, IMergeProcessorService
{
    public Task<SummarizerResponse> MergeSummariesAsync(IEnumerable<SummarizerResponse> partialSummaries, IEnumerable<TraceLog> originalLogs, IntentType inputType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<SummarizerResponse>> ProcessChunksAsync(List<LogChunk> chunks, ProcessingStrategy strategy, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
