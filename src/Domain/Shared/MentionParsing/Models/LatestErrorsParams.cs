namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters to fetch recent errors for a service/environment.
/// </summary>
public sealed record LatestErrorsParams : ICaseParameters
{
    /// <summary>
    /// Optional absolute start time (UTC) when specified.
    /// </summary>
    public string FromIso { get; init; } = string.Empty;

    /// <summary>
    /// Optional absolute end time (UTC) when specified.
    /// </summary>
    public string ToIso { get; init; } = string.Empty;

    /// <summary>
    /// Number of error items to return.
    /// </summary>
    public int TopN { get; init; } = 20;

    /// <summary>
    /// Target service name if provided.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Timezone context for time parsing.
    /// </summary>
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Deployment environment constraint.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
