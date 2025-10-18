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

    public async Task<string?> GetMessageContentAsync(string channelId, string messageTs, CancellationToken cancellationToken = default)
    {
        var response = await _slackApiClient.Conversations.Replies(
                    channelId,
                    messageTs,
                    limit: 1, // Chỉ lấy 1 tin nhắn
                    cancellationToken: cancellationToken
                );

        // Tin nhắn gốc luôn là tin nhắn đầu tiên trong danh sách `Messages` trả về
        var originalMessage = response?.Messages.FirstOrDefault();

        return originalMessage?.Text;
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
