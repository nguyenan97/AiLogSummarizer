using Domain.MentionParsing.Models;

namespace Application.Interfaces;

public interface IMentionParserService
{
    Task<MentionParsed> ParseAsync(string userText, CancellationToken cancellationToken = default);
}
