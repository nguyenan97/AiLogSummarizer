using Application.Interfaces;
using SlackNet;
using SlackNet.WebApi;

namespace Infrastructure.Services;

public class SlackChatService : IMessageSenderService
{
    private readonly ISlackApiClient _slackApiClient;

    public SlackChatService(ISlackApiClient slackApiClient)
    {
        _slackApiClient = slackApiClient;
    }

    public async Task SendMessageAsync(string channel, string message, string? threadTs = null)
    {
        var msg = new Message
        {
            Channel = channel,
            Text = message,
            ThreadTs = threadTs
        };

        await _slackApiClient.Chat.PostMessage(msg);
    }
}
