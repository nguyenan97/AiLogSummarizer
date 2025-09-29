namespace Application.Interfaces;

public interface ISlackChatService
{
    Task SendMessageAsync(string channel, string message, string? threadTs = null);
}