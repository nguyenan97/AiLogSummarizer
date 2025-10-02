using Application.Interfaces;
using Domain.Models;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.LogSources;

public class CompositeLogSource : ICompositeLogSource
{
    private readonly IServiceProvider _serviceProvider;
    public CompositeLogSource(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<TraceLog>> GetLogsAsync(GetLogModel model)
    {
        ILogSourceService? logSourceService = _serviceProvider.GetKeyedService<ILogSourceService>(model.Source);
        if (logSourceService == null)
            throw new NotSupportedException($"Log source '{model.Source}' is not supported.");
        if (logSourceService == null)
        {
            throw new NotSupportedException($"Log source type '{model.Source}' is not supported.");
        }
        var logs = await logSourceService.GetLogsAsync(model);
        return logs.ToList();
    }
}
