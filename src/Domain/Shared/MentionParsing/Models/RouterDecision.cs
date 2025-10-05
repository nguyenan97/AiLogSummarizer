namespace Domain.MentionParsing.Models;

public sealed record RouterDecision
{
    public IntentType Intent { get; init; } = IntentType.Unknown;
    public IntentCategory Category { get; init; } = IntentCategory.Unknown;
    public double Confidence { get; init; }
    public string? Suggestion { get; init; }
}