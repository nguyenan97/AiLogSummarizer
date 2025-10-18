using Application.Interfaces;
using Domain.MentionParsing.Models;
using Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mentions;

public record HandleAppMentionCommand(string Text, string Channel, string ThreadTs, bool UseHistory = false) : IRequest;

public class HandleAppMentionCommandHandler : IRequestHandler<HandleAppMentionCommand>
{
    private readonly IMessageSenderService _messageSenderService;
    private readonly IMentionParserService _mentionParserService;
    private readonly ICompositeLogSource _compositeLogSource;
    private readonly ISummarizerService _summarizerService;
    private readonly ILogger<HandleAppMentionCommandHandler> _logger;
    private readonly IHistoryLayerService _historyLayerService;

    public HandleAppMentionCommandHandler(
        IMessageSenderService messageSenderService,
        IMentionParserService mentionParserService,
        ICompositeLogSource compositeLogSource,
        ISummarizerService summarizerService,
        ILogger<HandleAppMentionCommandHandler> logger,
        IHistoryLayerService historyLayerService)
    {
        _messageSenderService = messageSenderService;
        _mentionParserService = mentionParserService;
        _compositeLogSource = compositeLogSource;
        _summarizerService = summarizerService;
        _logger = logger;
        _historyLayerService = historyLayerService;
    }

    public async Task Handle(HandleAppMentionCommand request, CancellationToken cancellationToken)
    {
        var conversationKey = request.UseHistory ? $"{request.Channel}:{request.ThreadTs}" : null;
        var parseResult = await _mentionParserService.ParseAsync(request.Text, conversationKey, cancellationToken);

        // TODO: Add email notification support for critical errors
        // Next step: Integrate SMTP service for automated error reports
        // Improvement: Implement MCP for dynamic AI model switching based on error severity
        // Send localized processing message with loading icon based on detected language
        var processingMessage = BuildProcessingMessage(parseResult!.Language);
        await _messageSenderService.SendMessageAsync(request.Channel, processingMessage, request.ThreadTs);

        if (parseResult.Intent == Domain.MentionParsing.Models.IntentType.Unknown)
        {
            var unknown = parseResult.Parameters as UnknownIntentParams ?? new UnknownIntentParams();
            await _messageSenderService.SendMessageAsync(
                request.Channel,
                unknown?.Message?.Trim() ?? string.Empty,
                request.ThreadTs);

            return;
        }

        Domain.Models.LogQueryContext query;
        if (parseResult.Parameters is LogQueryCaseParams q)
        {
            // New unified parameters already aligned with LogQueryContext.
            query = q.Query;
        }
        else
        {
            throw new Exception("Cannot parse to  LogQueryContext");
        }

        var logs = await _compositeLogSource.GetLogsAsync(query) ?? new List<TraceLog>();

        var summary = await _summarizerService.ProcessLogsAsync(logs, parseResult.Intent, parseResult.Language, cancellationToken);

        await _messageSenderService.SendMessageAsync(request.Channel, summary.RawMarkdown, request.ThreadTs);
        if (logs.Any() && !request.UseHistory)
        {
            try
            {
                await _historyLayerService.AddMemoryAsync(
                        userMessage: request.Text,
                        userId: $"{request.Channel}:{request.ThreadTs}",
                        assistantResponse: summary.RawMarkdown
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding memory");
            }
        }
    }

    private static string BuildProcessingMessage(InputLanguage language)
    {
        return language.GetDescription().StartsWith("Vietnamese")
            ? "⏳ Đã nhận yêu cầu — đang xử lý..."
            : "⏳ Request received — processing...";
    }
}
