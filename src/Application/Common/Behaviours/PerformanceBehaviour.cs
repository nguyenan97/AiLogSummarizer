using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public PerformanceBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var response = await next();
        var elapsed = DateTime.UtcNow - startTime;

        if (elapsed.TotalSeconds > 1)
        {
            _logger.LogWarning("Long running request: {Request} took {Elapsed} seconds", typeof(TRequest).Name, elapsed.TotalSeconds);
        }

        return response;
    }
}