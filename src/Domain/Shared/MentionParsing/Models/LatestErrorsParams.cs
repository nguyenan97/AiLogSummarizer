namespace Domain.MentionParsing.Models;

public sealed record LatestErrorsParams : ICaseParameters
{
    public int TopN { get; init; } = 20;
    public string? Service { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
    public string? Environment { get; init; }
}