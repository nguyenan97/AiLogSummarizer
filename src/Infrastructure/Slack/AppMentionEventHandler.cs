using Application.Features.Mentions;
using MediatR;
using SlackNet;
using SlackNet.Events;

namespace Infrastructure.Slack;

public class AppMentionEventHandler : IEventHandler<AppMention>
{
    private readonly IMediator _mediator;

    public AppMentionEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(AppMention slackEvent)
    {
        // Use thread_ts if present, otherwise start a new thread under the mention message (ts)
        var threadTs = slackEvent.ThreadTs ?? slackEvent.Ts;

        await _mediator.Send(new HandleAppMentionCommand(
            Text: slackEvent.Text ?? string.Empty,
            Channel: slackEvent.Channel,
            ThreadTs: threadTs
        ));
    }
}
