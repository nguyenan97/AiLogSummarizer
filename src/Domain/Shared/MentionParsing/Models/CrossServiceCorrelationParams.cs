using System.Collections.Generic;
using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters to correlate events across services using shared attributes (e.g., orderId).
/// </summary>
public sealed record CrossServiceCorrelationParams : ICaseParameters
{
    /// <summary>
    /// Key/value attributes to join across services, e.g., {"orderId":"9981"}.
    /// </summary>
    public IReadOnlyDictionary<string, string> CorrelationAttributes { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Relative lookback window for correlation (default 24h).
    /// </summary>
    public string Lookback { get; init; } = "PT24H";

    /// <summary>
    /// Optional environment constraint (e.g., prod).
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Timezone for resolving time expressions.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
