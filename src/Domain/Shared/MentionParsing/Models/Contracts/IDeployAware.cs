namespace Domain.MentionParsing.Models.Contracts;

/// <summary>
/// Contract for cases comparing before/after a deployment marker.
/// </summary>
public interface IDeployAware
{
    /// <summary>
    /// Deployment marker (tag/commit/PR) to compare around.
    /// </summary>
    string DeployMarker { get; }

    /// <summary>
    /// Lookback window before the deploy (ISO8601 duration).
    /// </summary>
    string LookbackBefore { get; }

    /// <summary>
    /// Lookback window after the deploy (ISO8601 duration).
    /// </summary>
    string LookbackAfter { get; }
}
