using Domain.Shared;

namespace Application.Interfaces;

public interface IMentionParserService
{
    Task<ParsedMentionResult> ParseMentionAsync(string text);
}

