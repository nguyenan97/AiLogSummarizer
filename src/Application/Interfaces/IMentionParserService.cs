using Domain.MentionParsing.Models;

namespace Application.Interfaces;

public interface IMentionParserService
{
    Task<MentionParsed> ParseAsync(string userText, string? conversationKey = null, CancellationToken cancellationToken = default);
    string ReadPrompt(string fileName);
}
