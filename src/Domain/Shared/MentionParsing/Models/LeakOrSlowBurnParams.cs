namespace Domain.MentionParsing.Models;

public sealed record LeakOrSlowBurnParams : ICaseParameters
{
    public string Symptom { get; init; } = string.Empty;
    public string Lookback { get; init; } = "P7D";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}
