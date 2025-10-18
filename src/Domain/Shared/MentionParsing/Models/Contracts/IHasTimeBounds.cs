namespace Domain.MentionParsing.Models.Contracts;

/// <summary>
/// Contract for cases that include explicit time scoping.
/// Implementations should surface absolute bounds and/or a relative lookback.
/// Values may be provided directly by the model or forwarded from Context.
/// </summary>
public interface IHasTimeBounds
{
    /// <summary>
    /// Absolute start time in ISO8601 (UTC). May be null when using lookback.
    /// </summary>
    string? FromIso { get; }

    /// <summary>
    /// Absolute end time in ISO8601 (UTC). May be null when using lookback.
    /// </summary>
    string? ToIso { get; }

    /// <summary>
    /// Relative ISO8601 duration (e.g., PT2H) when using lookback.
    /// May be null when using absolute bounds.
    /// </summary>
    string? Lookback { get; }

    /// <summary>
    /// IANA time zone used for time resolution (e.g., Asia/Ho_Chi_Minh).
    /// </summary>
    string TimeZone { get; }
}
