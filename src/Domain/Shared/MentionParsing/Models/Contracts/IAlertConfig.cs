namespace Domain.MentionParsing.Models.Contracts;

/// <summary>
/// Contract for temporary alert/monitor configuration requests.
/// </summary>
public interface IAlertConfig
{
    /// <summary>
    /// Human-friendly alert expression (e.g., "5xx > 2% for 10m").
    /// </summary>
    string Condition { get; }

    /// <summary>
    /// Active duration of the temporary alert (ISO8601, e.g., PT2H).
    /// </summary>
    string Duration { get; }

    /// <summary>
    /// Optional target service.
    /// </summary>
    string? Service { get; }

    /// <summary>
    /// Optional target environment.
    /// </summary>
    string? Environment { get; }

    /// <summary>
    /// Delivery channel, e.g., slack-thread.
    /// </summary>
    string Channel { get; }

    /// <summary>
    /// Optional Datadog monitor/log query DSL if available.
    /// </summary>
    string? DatadogQuery { get; }
}
