using Application.Interfaces;
using Domain.Shared;

namespace Infrastructure.Services;

public class SummarizerService : ISummarizerService
{
    public Task<string> SummarizeAsync(IEnumerable<TraceLog> logs, DesiredOutputType outputType)
    {
        // TODO: Implement summarization logic
        return Task.FromResult("Sample summary");
    }
}