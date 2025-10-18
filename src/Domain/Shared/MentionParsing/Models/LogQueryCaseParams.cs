using Domain.Models;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Unified parameters for all log-related intents, aligned with LogQueryContext.
/// Keeps backward-compatibility via Context projection for existing consumers.
/// </summary>
public sealed record LogQueryCaseParams : ICaseParameters
{
    /// <summary>
    /// The unified query model used by log sources.
    /// </summary>
    public LogQueryContext Query { get; init; } = new();

    /// <summary>
    /// Backward-compatible context projection from the unified query.
    /// Only a subset of fields can be mapped losslessly.
    /// </summary>
    public CaseContext Context => new()
    {
        Environment = Query.Environment,
        From = Query.From,
        To = Query.To,
        Host = Query.Host,
        Keywords = Query.Keywords,
        Levels = Query.Levels,
        ServiceNames = Query.ServiceNames,
        Source = Shared.SourceType.Datadog,
        Limit = Query.Limit,
        Tags = Query.Tags
    };
}

