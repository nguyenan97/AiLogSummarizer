using Application.Interfaces;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Infrastructure.Services;

public class ConsoleChatService : IMessageSenderService
{

    private readonly ILogger _logger;
    public ConsoleChatService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ConsoleChatService>();
    }

    public async Task SendMessageAsync(string channel, string message, string? threadTs = null)
    {

        await Task.Run(() =>
        {
            _logger.LogWarning(message);
        });

       
        
    }
}
