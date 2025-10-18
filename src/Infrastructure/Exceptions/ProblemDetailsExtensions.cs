using System.Net;
using System.Reflection;
using Domain.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Exceptions;

public static class ProblemDetailsExtensions
{
    private const string DefaultInternalServerErrorType = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public static (ProblemDetails ProblemDetails, int StatusCode, string CorrelationId) MapToProblemDetails(
        HttpContext httpContext,
        Exception exception,
        IHostEnvironment environment)
    {
        int statusCode = GetStatusCode(exception);

        string type = statusCode >= 500
            ? DefaultInternalServerErrorType
            : GetTypeLinkForStatus(statusCode);

        string title = ReasonPhrases.GetReasonPhrase(statusCode);

        string correlationId = EnsureCorrelationId(httpContext);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Type = type,
            Title = title,
            Detail = GetDetail(exception, statusCode),
            Instance = httpContext.Request.Path
        };

        problem.Extensions["correlationId"] = correlationId;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        // Validation errors (our Domain ValidationException or FluentValidation if present)
        if (TryGetValidationErrors(exception, out var errors))
        {
            problem.Extensions["errors"] = errors;
        }

        // In Development, include exception details (stack trace)
        if (environment.IsDevelopment())
        {
            problem.Extensions["exception"] = new
            {
                message = exception.Message,
                details = exception.ToString()
            };
        }

        return (problem, statusCode, correlationId);
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        // 400 Bad Request
        BadHttpRequestException => StatusCodes.Status400BadRequest,
        Domain.Common.Exceptions.ValidationException => StatusCodes.Status400BadRequest,
        _ when IsFluentValidationException(exception) => StatusCodes.Status400BadRequest,

        // 401 Unauthorized
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

        // 404 Not Found
        KeyNotFoundException => StatusCodes.Status404NotFound,
        EntityNotFoundException => StatusCodes.Status404NotFound,

        // 409 Conflict
        DomainException => StatusCodes.Status409Conflict,

        // 501 Not Implemented
        NotImplementedException => StatusCodes.Status501NotImplemented,

        // 500 Internal Server Error (default)
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTypeLinkForStatus(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        StatusCodes.Status501NotImplemented => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
        _ => DefaultInternalServerErrorType
    };

    private static string GetDetail(Exception exception, int statusCode)
    {
        // Avoid leaking sensitive details in production paths: messages for client errors, generic for 5xx
        return statusCode >= 500
            ? "An unexpected error occurred."
            : exception.Message;
    }

    private static string EnsureCorrelationId(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue(CorrelationIdHeader, out var incoming) && !string.IsNullOrWhiteSpace(incoming))
        {
            // Forward to response header for convenience
            httpContext.Response.Headers[CorrelationIdHeader] = incoming.ToString();
            return incoming.ToString();
        }

        var generated = Guid.NewGuid().ToString("N");
        httpContext.Response.Headers[CorrelationIdHeader] = generated;
        return generated;
    }

    private static bool TryGetValidationErrors(Exception ex, out IDictionary<string, string[]> errors)
    {
        // Domain ValidationException
        if (ex is Domain.Common.Exceptions.ValidationException dv && dv.Errors is not null)
        {
            errors = dv.Errors;
            return true;
        }

        // FluentValidation.ValidationException (access via reflection to avoid direct package dependency here)
        if (IsFluentValidationException(ex))
        {
            errors = ExtractFluentValidationErrors(ex);
            return true;
        }

        errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        return false;
    }

    private static bool IsFluentValidationException(Exception ex)
        => ex.GetType().FullName == "FluentValidation.ValidationException";

    private static IDictionary<string, string[]> ExtractFluentValidationErrors(object validationException)
    {
        // Expecting property: IEnumerable<ValidationFailure> Errors { get; }
        var errorsProp = validationException.GetType().GetProperty("Errors", BindingFlags.Instance | BindingFlags.Public);
        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (errorsProp is null)
        {
            return result;
        }

        var errorsEnumerable = errorsProp.GetValue(validationException) as System.Collections.IEnumerable;
        if (errorsEnumerable is null)
        {
            return result;
        }

        foreach (var failure in errorsEnumerable)
        {
            // ValidationFailure has properties: PropertyName, ErrorMessage
            var propertyName = failure?.GetType().GetProperty("PropertyName")?.GetValue(failure) as string ?? string.Empty;
            var errorMessage = failure?.GetType().GetProperty("ErrorMessage")?.GetValue(failure) as string ?? string.Empty;
            if (string.IsNullOrEmpty(propertyName)) propertyName = string.Empty;

            if (!result.TryGetValue(propertyName, out var list))
            {
                result[propertyName] = new[] { errorMessage };
            }
            else
            {
                result[propertyName] = list.Concat(new[] { errorMessage }).ToArray();
            }
        }

        return result;
    }
}
