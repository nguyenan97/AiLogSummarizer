using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters to fetch errors within a specific absolute time range.
/// </summary>
public sealed record TimeRangeErrorsParams : ICaseParameters
{
    /// <summary>
    /// Start time in ISO8601 (UTC).
    /// </summary>
    public string FromIso { get; init; } = string.Empty;

    /// <summary>
    /// End time in ISO8601 (UTC).
    /// </summary>
    public string ToIso { get; init; } = string.Empty;

    /// <summary>
    /// Timezone used to parse natural language times.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Target service when specified.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Target environment when specified.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Optional serverity filters (e.g., ["error","critical"]).
    /// </summary>
    public string[]? Severities { get; init; }

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
