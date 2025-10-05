namespace Domain.MentionParsing.Models;

public sealed record MentionParsed
{
    public IntentType Intent { get; init; } = IntentType.Unknown;
    public IntentCategory Category { get; init; } = IntentCategory.Unknown;
    public ICaseParameters Parameters { get; init; } = new GeneralAnalysisParams();
    public string? Suggestion { get; init; }
}