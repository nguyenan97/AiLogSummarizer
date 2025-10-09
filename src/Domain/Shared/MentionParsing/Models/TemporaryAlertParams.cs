using Domain.MentionParsing.Models.Contracts;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters describing a temporary alert/monitor request.
/// </summary>
public sealed record TemporaryAlertParams : ICaseParameters, IAlertConfig
{
    /// <summary>
    /// Human-friendly alert expression (e.g., "5xx > 2% for 10m").
    /// </summary>
    public string Condition { get; init; } = string.Empty;

    /// <summary>
    /// Overall active duration of the temporary alert (e.g., "PT2H").
    /// </summary>
    public string Duration { get; init; } = "PT2H";

    /// <summary>
    /// Optional target service.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional target environment.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Delivery channel (default "slack-thread").
    /// </summary>
    public string Channel { get; init; } = "slack-thread";

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// May contain a parsed DatadogQuery for the monitor.
    /// </summary>
    public CaseContext Context { get; init; } = new();

    // IAlertConfig delegation
    public string? DatadogQuery => Context.DatadogQuery;
}
