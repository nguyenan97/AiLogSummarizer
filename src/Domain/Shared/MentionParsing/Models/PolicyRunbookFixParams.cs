namespace Domain.MentionParsing.Models;

public sealed record PolicyRunbookFixParams : ICaseParameters
{
    public string Issue { get; init; } = string.Empty;
    public string Severity { get; init; } = "medium";
    public string? Service { get; init; }
    public string? Environment { get; init; }
}