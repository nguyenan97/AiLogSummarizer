using Application.Interfaces;
using Domain.Shared;

namespace Infrastructure.Services;

public class MentionParserService : IMentionParserService
{
    public Task<ParsedMentionResult> ParseMentionAsync(string text)
    {
        // TODO: Implement parsing logic
        return Task.FromResult(new ParsedMentionResult(
            IntentType.Summarize,
            new TimeRange(DateTime.Now.AddHours(-1), DateTime.Now),
            SourceType.Datadog,
            DesiredOutputType.Text,
            true));
    }
}