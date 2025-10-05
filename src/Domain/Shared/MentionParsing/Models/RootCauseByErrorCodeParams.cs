namespace Domain.MentionParsing.Models;

public sealed record RootCauseByErrorCodeParams : ICaseParameters
{
    public string ErrorCode { get; init; } = string.Empty;
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string TimeWindow { get; init; } = "PT24H";
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}
