namespace Domain.MentionParsing.Models;

public sealed record SlaSloMonitoringParams : ICaseParameters
{
    public string Period { get; init; } = "PT24H";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public double? TargetSLO { get; init; }
}