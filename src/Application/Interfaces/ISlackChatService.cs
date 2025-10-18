namespace Application.Interfaces;

public interface IMessageSenderService
{
    Task SendMessageAsync(string channel, string message, string? threadTs = null);

    /// <summary>
    /// Lấy nội dung của một tin nhắn cụ thể trong một thread.
    /// </summary>
    Task<string?> GetMessageContentAsync(string channelId, string messageTs, CancellationToken cancellationToken = default);
}