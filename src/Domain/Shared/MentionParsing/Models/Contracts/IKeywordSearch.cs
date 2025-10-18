namespace Domain.MentionParsing.Models.Contracts;

/// <summary>
/// Contract for keyword- or exception-based log searches.
/// </summary>
public interface IKeywordSearch
{
    /// <summary>
    /// Search keywords.
    /// </summary>
    string[] Keywords { get; }

    /// <summary>
    /// Optional exception class/type filter.
    /// </summary>
    string? ExceptionType { get; }

    /// <summary>
    /// Relative lookback window for the search.
    /// </summary>
    string Lookback { get; }
}
