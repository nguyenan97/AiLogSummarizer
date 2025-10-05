namespace Domain.MentionParsing.Models;

public sealed record TimeRangeErrorsParams : ICaseParameters
{
    public string FromIso { get; init; } = string.Empty;
    public string ToIso { get; init; } = string.Empty;
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string[]? Severities { get; init; }
}