namespace Domain.MentionParsing.Models;

public sealed record TemporaryAlertParams : ICaseParameters
{
    public string Condition { get; init; } = string.Empty;
    public string Duration { get; init; } = "PT2H";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string Channel { get; init; } = "slack-thread";
}