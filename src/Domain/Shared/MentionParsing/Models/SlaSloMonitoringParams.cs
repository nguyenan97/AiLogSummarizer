namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters for SLA/SLO checks and error budget reporting.
/// </summary>
public sealed record SlaSloMonitoringParams : ICaseParameters
{
    /// <summary>
    /// Monitoring period as ISO8601 duration (e.g., PT24H, P7D).
    /// </summary>
    public string Period { get; init; } = "PT24H";

    /// <summary>
    /// Optional service scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment scope.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Target SLO (e.g., 99.9 or 0.999). Optional.
    /// </summary>
    public double? TargetSLO { get; init; }

    /// <summary>
    /// Objective type (e.g., "latency", "error_rate", "availability"). Optional.
    /// </summary>
    public string? ObjectiveType { get; init; }

    /// <summary>
    /// Metric name or indicator (e.g., "http.server.duration", "5xx"). Optional.
    /// </summary>
    public string? MetricName { get; init; }

    /// <summary>
    /// Aggregation or percentile (e.g., "p95", "avg"). Optional.
    /// </summary>
    public string? Aggregation { get; init; }

    /// <summary>
    /// Optional window used to compute remaining error budget (e.g., "today").
    /// </summary>
    public string? ErrorBudgetWindow { get; init; }

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
