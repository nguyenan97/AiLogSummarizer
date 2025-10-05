using System.Collections.Generic;

namespace Domain.MentionParsing.Models;

public sealed record GeneralAnalysisParams : ICaseParameters
{
    public string DetectedIntent { get; init; } = string.Empty;
    public Dictionary<string, string> Fields { get; init; } = new();
    public string? Notes { get; init; }
}