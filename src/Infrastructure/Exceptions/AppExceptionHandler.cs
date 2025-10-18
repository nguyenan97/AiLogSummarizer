using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Exceptions;

public sealed class AppExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AppExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    private static readonly EventId UnhandledExceptionEvent = new(1000, nameof(UnhandledExceptionEvent));

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (problemDetails, statusCode, correlationId) = ProblemDetailsExtensions.MapToProblemDetails(httpContext, exception, environment);

        // Log with standardized context
        logger.LogError(
            UnhandledExceptionEvent,
            exception,
            "Unhandled exception. TraceId={TraceId}, CorrelationId={CorrelationId}, StatusCode={StatusCode}",
            httpContext.TraceIdentifier,
            correlationId,
            statusCode);

        httpContext.Response.StatusCode = statusCode;

        // Write RFC7807 response via built-in ProblemDetailsService
        var wrote = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        return wrote; // true indicates the exception has been handled
    }
}
