using System;

namespace Domain.MentionParsing.Models;

public sealed record SearchByKeywordExceptionParams : ICaseParameters
{
    public string[] Keywords { get; init; } = Array.Empty<string>();
    public string? ExceptionType { get; init; }
    public string Lookback { get; init; } = "PT24H";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}
