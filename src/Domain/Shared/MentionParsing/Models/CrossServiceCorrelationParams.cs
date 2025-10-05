using System;

namespace Domain.MentionParsing.Models;

public sealed record CrossServiceCorrelationParams : ICaseParameters
{
    public string[] CorrelationKeys { get; init; } = Array.Empty<string>();
    public string Lookback { get; init; } = "PT24H";
    public string? Environment { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}
