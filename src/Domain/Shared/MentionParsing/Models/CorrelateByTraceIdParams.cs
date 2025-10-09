using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters for correlating events across services using a TraceId (and optional SpanId).
/// </summary>
public sealed record CorrelateByTraceIdParams : ICaseParameters, ITraceCorrelatable, IHasTimeBounds
{
    /// <summary>
    /// Primary distributed TraceId to correlate across services.
    /// </summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>
    /// Optional specific service hint to narrow search scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment hint (e.g., prod, staging).
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Optional SpanId tied to the trace for more precise correlation.
    /// </summary>
    public string? SpanId { get; init; }

    /// <summary>
    /// Relative lookback window, defaults to last 6 hours.
    /// </summary>
    public string Lookback { get; init; } = "PT6H";

    /// <summary>
    /// Timezone used to resolve relative time expressions.
    /// </summary>
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();

    // IHasTimeBounds delegation
    public string? FromIso => Context.FromIso;
    public string? ToIso => Context.ToIso;
}
