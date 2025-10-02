using Domain.Models;
using Domain.Shared;

namespace Application.Interfaces;

public interface ILogSourceService
{
    Task<IEnumerable<TraceLog>> GetLogsAsync(GetLogModel model);
}