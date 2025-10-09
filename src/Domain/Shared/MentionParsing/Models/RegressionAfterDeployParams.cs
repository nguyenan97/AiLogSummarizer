using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters to compare signals before/after a deployment marker.
/// </summary>
public sealed record RegressionAfterDeployParams : ICaseParameters, IDeployAware, IHasTimeBounds
{
    /// <summary>
    /// Deployment tag/commit/PR marker identifying the release.
    /// </summary>
    public string DeployMarker { get; init; } = string.Empty;

    /// <summary>
    /// Lookback window before the deploy (e.g., "PT24H").
    /// </summary>
    public string LookbackBefore { get; init; } = "PT24H";

    /// <summary>
    /// Lookback window after the deploy (e.g., "PT24H").
    /// </summary>
    public string LookbackAfter { get; init; } = "PT24H";

    /// <summary>
    /// Optional service scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment scope.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Timezone for time resolution.
    /// </summary>
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();

    // IHasTimeBounds delegation
    public string? FromIso => Context.FromIso;
    public string? ToIso => Context.ToIso;
    public string? Lookback => null;
}
