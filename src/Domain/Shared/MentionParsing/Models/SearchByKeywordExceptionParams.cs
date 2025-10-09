using System;
using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters for keyword/exception-based log searching.
/// </summary>
public sealed record SearchByKeywordExceptionParams : ICaseParameters, IKeywordSearch, IHasTimeBounds
{
    /// <summary>
    /// Search keywords (deduplicated, order preserved if possible).
    /// </summary>
    public string[] Keywords { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Optional exception type (e.g., NullReferenceException).
    /// </summary>
    public string? ExceptionType { get; init; }

    /// <summary>
    /// Relative lookback duration (e.g., PT2H).
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
    /// Timezone used to resolve any natural time expressions.
    /// </summary>
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();

    // IHasTimeBounds delegation
    public string? FromIso => Context.FromIso;
    public string? ToIso => Context.ToIso;
}
