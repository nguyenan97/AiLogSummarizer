namespace Domain.MentionParsing.Models.Contracts;

/// <summary>
/// Contract for cases driven by a specific error/business code.
/// </summary>
public interface IErrorCodeCase
{
    /// <summary>
    /// The primary error or business code (e.g., ERR-1001, 504).
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    /// Relative ISO8601 window around the incident (e.g., PT1H).
    /// </summary>
    string TimeWindow { get; }
}
