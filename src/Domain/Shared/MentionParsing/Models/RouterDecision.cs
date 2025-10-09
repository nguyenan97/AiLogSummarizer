namespace Domain.MentionParsing.Models;

public sealed record RouterDecision
{
    public IntentType Intent { get; init; } = IntentType.Unknown;
    public IntentDetail IntentDetail { get; init; } = IntentDetail.Unknown;
}