using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Shared;
using MediatR;

namespace Application.Features.Mentions;

public record HandleAppMentionCommand(string Text, string Channel, string ThreadTs) : IRequest;

public class HandleAppMentionCommandHandler : IRequestHandler<HandleAppMentionCommand>
{
    private static readonly CultureInfo ParserCulture = CultureInfo.InvariantCulture;

    private readonly ISlackChatService _slackChatService;
    private readonly IMentionParserService _mentionParserService;
    private readonly ICompositeLogSource _compositeLogSource;
    private readonly ISummarizerService _summarizerService;

    public HandleAppMentionCommandHandler(
        ISlackChatService slackChatService,
        IMentionParserService mentionParserService,
         ICompositeLogSource compositeLogSource,
        ISummarizerService summarizerService)
    {
        _slackChatService = slackChatService;
        _mentionParserService = mentionParserService;
        _compositeLogSource = compositeLogSource;
        _summarizerService = summarizerService;
    }

    public async Task Handle(HandleAppMentionCommand request, CancellationToken cancellationToken)
    {
        await _slackChatService.SendMessageAsync(
            request.Channel,
            "Request received — processing…",
            request.ThreadTs);

        try
        {
            var parseResult = await _mentionParserService.ParseAsync(request.Text, cancellationToken);

            var logs = await _compositeLogSource.GetLogsAsync(new Domain.Models.GetLogModel
            {
                StartTime = DateTimeOffset.UtcNow,
                EndTime = DateTimeOffset.UtcNow,
                Source = SourceType.Datadog,
            });

            var summary = await _summarizerService.SummarizeAsync(logs, DesiredOutputType.Text);

            await _slackChatService.SendMessageAsync(request.Channel, summary, request.ThreadTs);
        }
        catch
        {
            await _slackChatService.SendMessageAsync(
                request.Channel,
                "An unexpected error occurred. Please try again later.",
                request.ThreadTs);
        }
    }
}
