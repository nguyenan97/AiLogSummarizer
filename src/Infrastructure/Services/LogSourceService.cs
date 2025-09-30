using Application.Interfaces;
using Domain.Shared;

namespace Infrastructure.Services;

public class LogSourceService : ILogSourceService
{
    public Task<IEnumerable<TraceLog>> GetLogsAsync(TimeRange timeRange, SourceType source)
    {
        var logs = new List<TraceLog>
        {
            new(DateTimeOffset.UtcNow, "Sample log message", "INFO", source.ToString())
        };

        return Task.FromResult<IEnumerable<TraceLog>>(logs);
    }
}
