namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters for freeform Q&A over logs/telemetry.
/// </summary>
public sealed record FreeformLogQaParams : ICaseParameters
{
    /// <summary>
    /// The user's question captured verbatim.
    /// </summary>
    public string Question { get; init; } = string.Empty;

    /// <summary>
    /// Relative lookback for the query (default 24h).
    /// </summary>
    public string Lookback { get; init; } = "PT24H";

    /// <summary>
    /// Optional service scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment scope.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Timezone for time expressions.
    /// </summary>
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
