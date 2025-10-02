using System.Threading;
using System.Threading.Tasks;
using Domain.Shared;

namespace Application.Interfaces;

public interface IMentionParserService
{
    Task<MentionParseResult> ParseMentionAsync(string text, CancellationToken cancellationToken = default);
}
