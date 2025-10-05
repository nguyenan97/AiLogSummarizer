using System;

namespace Domain.MentionParsing.Models;

public sealed record UnknownIntentParams : ICaseParameters
{
    public string[] Samples { get; init; } = Array.Empty<string>();
    public string? Notes { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}