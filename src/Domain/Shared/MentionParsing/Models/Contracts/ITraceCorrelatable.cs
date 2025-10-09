namespace Domain.MentionParsing.Models.Contracts;

/// <summary>
/// Contract for cases that carry trace/span identifiers for cross-service correlation.
/// </summary>
public interface ITraceCorrelatable
{
    /// <summary>
    /// Primary distributed trace identifier.
    /// </summary>
    string? TraceId { get; }

    /// <summary>
    /// Optional span id associated with the TraceId.
    /// </summary>
    string? SpanId { get; }
}
