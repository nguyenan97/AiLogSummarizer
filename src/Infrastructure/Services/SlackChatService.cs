using Application.Interfaces;
using SlackNet;

namespace Infrastructure.Services;

public class SlackChatService : ISlackChatService
{
    private readonly ISlackApiClient _slackApiClient;

    public SlackChatService(ISlackApiClient slackApiClient)
    {
        _slackApiClient = slackApiClient;
    }

    public async Task SendMessageAsync(string channel, string message, string? threadTs = null)
    {
        // TODO: Implement sending message to Slack
        await Task.CompletedTask;
    }
}