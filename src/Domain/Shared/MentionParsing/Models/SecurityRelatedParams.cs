namespace Domain.MentionParsing.Models;

public sealed record SecurityRelatedParams : ICaseParameters
{
    public string Concern { get; init; } = string.Empty;
    public string Lookback { get; init; } = "PT24H";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}
