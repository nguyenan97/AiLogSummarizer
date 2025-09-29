using Application.Interfaces;
using Domain.Shared;
using MediatR;

namespace Application.Features.Mentions;

public record HandleAppMentionCommand(string Text, string Channel, string ThreadTs) : IRequest;

public class HandleAppMentionCommandHandler : IRequestHandler<HandleAppMentionCommand>
{
    private readonly ISlackChatService _slackChatService;
    private readonly IMentionParserService _mentionParserService;
    private readonly ILogSourceService _logSourceService;
    private readonly ISummarizerService _summarizerService;

    public HandleAppMentionCommandHandler(
        ISlackChatService slackChatService,
        IMentionParserService mentionParserService,
        ILogSourceService logSourceService,
        ISummarizerService summarizerService)
    {
        _slackChatService = slackChatService;
        _mentionParserService = mentionParserService;
        _logSourceService = logSourceService;
        _summarizerService = summarizerService;
    }

    public async Task Handle(HandleAppMentionCommand request, CancellationToken cancellationToken)
    {
        await _slackChatService.SendMessageAsync(
            request.Channel,
            "Đã nhận yêu cầu — đang xử lý…",
            request.ThreadTs);

        try
        {
            // 2) Parse the mention text (mock for now)
            var parseResult = await _mentionParserService.ParseMentionAsync(request.Text);

            if (!parseResult.IsValid)
            {
                var suggestion = parseResult.ErrorMessage ?? "Cú pháp chưa đúng. Ví dụ: `@bot summarize last 15m from datadog`";
                await _slackChatService.SendMessageAsync(request.Channel, suggestion, request.ThreadTs);
                return;
            }

            // 3) Fetch logs/trace from source (mock Datadog)
            IEnumerable<TraceLog> logs = await _logSourceService.GetLogsAsync(parseResult.TimeRange, parseResult.Source);

            // 4) Summarize (mock AI)
            var summary = await _summarizerService.SummarizeAsync(logs, parseResult.OutputType);

            // 5) Reply with the summarized result in the same thread
            await _slackChatService.SendMessageAsync(request.Channel, summary, request.ThreadTs);
        }
        catch
        {
            // 6) Unexpected error fallback
            await _slackChatService.SendMessageAsync(
                request.Channel,
                "An unexpected error occurred. Please try again later.",
                request.ThreadTs);
        }
    }
}

