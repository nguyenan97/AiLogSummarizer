using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;

namespace Application.Interfaces;

public interface ISummarizerService
{
    Task<SummarizerResponse> ProcessLogsAsync(
        IEnumerable<TraceLog> logs,
        IntentType intent,
        InputLanguage language = InputLanguage.Vietnamese,
        CancellationToken cancellationToken = default);
}
public interface IChunkProcessorService
{
    Task<List<SummarizerResponse>> ProcessChunksAsync(
        List<LogChunk> chunks,
        ProcessingStrategy strategy,
        CancellationToken cancellationToken = default);
}

public interface IMergeProcessorService
{
    Task<SummarizerResponse> MergeSummariesAsync(
        IEnumerable<SummarizerResponse> partialSummaries,
        IEnumerable<TraceLog> originalLogs,
        IntentType inputType,
        InputLanguage language = InputLanguage.Vietnamese,
        CancellationToken cancellationToken = default);
}