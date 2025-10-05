namespace Domain.MentionParsing.Models;

public sealed record CorrelateByTraceIdParams : ICaseParameters
{
    public string TraceId { get; init; } = string.Empty;
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string? SpanId { get; init; }
    public string Lookback { get; init; } = "PT6H";
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}
