namespace Application.Interfaces;

public interface IMessageSenderService
{
    Task SendMessageAsync(string channel, string message, string? threadTs = null);
}