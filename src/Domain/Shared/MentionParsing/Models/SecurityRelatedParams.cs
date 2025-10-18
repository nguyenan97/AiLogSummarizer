namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters for security-related inquiries (e.g., brute-force, SQL injection).
/// </summary>
public sealed record SecurityRelatedParams : ICaseParameters
{
    /// <summary>
    /// Security concern summary captured as stated by the user.
    /// </summary>
    public string Concern { get; init; } = string.Empty;

    /// <summary>
    /// Relative lookback window for investigation.
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
    /// Timezone for time normalization.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
