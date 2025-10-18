using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters to investigate suspected resource leaks or slow-burn issues.
/// </summary>
public sealed record LeakOrSlowBurnParams : ICaseParameters
{
    /// <summary>
    /// The primary symptom as stated, e.g., "memory leak", "increasing latency".
    /// </summary>
    public string Symptom { get; init; } = string.Empty;

    /// <summary>
    /// Lookback window for the investigation (default 7 days).
    /// </summary>
    public string Lookback { get; init; } = "P7D";

    /// <summary>
    /// Optional service scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment scope.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Timezone used to resolve time expressions.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
