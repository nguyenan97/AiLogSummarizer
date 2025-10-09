using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters to analyze root cause by a specific error/business code.
/// </summary>
public sealed record RootCauseByErrorCodeParams : ICaseParameters, IErrorCodeCase, IHasTimeBounds
{
    /// <summary>
    /// Primary error or business code token (e.g., ERR-1001, 504).
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>
    /// Optional service scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment scope.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Relative time window to search around the reference (default 24h).
    /// </summary>
    public string TimeWindow { get; init; } = "PT24H";

    /// <summary>
    /// Timezone for resolving time expressions.
    /// </summary>
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();

    // IHasTimeBounds delegation
    public string? FromIso => Context.FromIso;
    public string? ToIso => Context.ToIso;
    public string? Lookback => TimeWindow;
}
