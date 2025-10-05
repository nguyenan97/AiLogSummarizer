namespace Domain.MentionParsing.Models;

public sealed record ManagerReportParams : ICaseParameters
{
    public string Period { get; init; } = string.Empty;
    public string Audience { get; init; } = "manager";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public bool IncludeSLA { get; init; } = true;
}