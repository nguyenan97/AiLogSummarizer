using Domain.Shared;

namespace Application.Interfaces;

public interface ISummarizerService
{
    Task<string> SummarizeAsync(IEnumerable<TraceLog> logs, DesiredOutputType outputType);
}