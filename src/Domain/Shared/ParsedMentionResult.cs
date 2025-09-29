using Domain.Shared;

namespace Domain.Shared;

public record ParsedMentionResult(
    IntentType Intent,
    TimeRange TimeRange,
    SourceType Source,
    DesiredOutputType OutputType,
    bool IsValid,
    string? ErrorMessage = null
);
