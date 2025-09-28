using Domain.Shared;

namespace Application.Interfaces;

public interface ILogSourceService
{
    Task<IEnumerable<TraceLog>> GetLogsAsync(TimeRange timeRange, SourceType source);
}