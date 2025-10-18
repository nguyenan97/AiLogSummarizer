using Application.Features.Mentions;
using Application.Interfaces;
using MediatR;
using SlackNet;
using SlackNet.Events;

namespace Infrastructure.Slack;

public class AppMentionEventHandler : IEventHandler<AppMention>
{
    private readonly IMediator _mediator;
    private readonly IMessageSenderService _messageSenderService;

    public AppMentionEventHandler(IMediator mediator, IMessageSenderService messageSenderService)
    {
        _mediator = mediator;
        _messageSenderService = messageSenderService;
    }

    public async Task Handle(AppMention slackEvent)
    {
        // Use thread_ts if present, otherwise start a new thread under the mention message (ts)
        var isInThread = !string.IsNullOrEmpty(slackEvent.ThreadTs);
        var threadTs = slackEvent.ThreadTs ?? slackEvent.Ts;

        try
        {
            await _mediator.Send(new HandleAppMentionCommand(
                Text: slackEvent.Text ?? string.Empty,
                Channel: slackEvent.Channel,
                ThreadTs: threadTs,
                UseHistory: isInThread
            ));
        }
        catch (Exception ex)
        {
            // Fallback: still inform in Slack thread about the failure
            await _messageSenderService.SendMessageAsync(
                slackEvent.Channel,
                $":warning: Something went wrong: {ex.Message}",
                threadTs);
        }
    }
}
