namespace Domain.MentionParsing.Models;

public sealed record MentionParsed
{
    public IntentType Intent { get; init; } = IntentType.Unknown;
    public IntentDetail IntentDetail { get; init; } = IntentDetail.Unknown;
    public ICaseParameters Parameters { get; init; } = new GeneralAnalysisParams();
    public InputLanguage Language { get; init; } = InputLanguage.Unknown;
}
