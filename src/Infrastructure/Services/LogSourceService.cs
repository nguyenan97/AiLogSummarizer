using Application.Interfaces;
using Domain.Shared;

namespace Infrastructure.Services;

public class LogSourceService : ILogSourceService
{
    public Task<IEnumerable<TraceLog>> GetLogsAsync(TimeRange timeRange, SourceType source)
    {
        // TODO: Implement fetching logs from source
        return Task.FromResult<IEnumerable<TraceLog>>(new List<TraceLog>
        {
            new TraceLog(DateTime.Now, "Sample log message", "INFO", "Datadog")
        });
    }
}