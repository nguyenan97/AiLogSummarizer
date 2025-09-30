using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Shared;
using MediatR;

namespace Application.Features.Mentions;

public record HandleAppMentionCommand(string Text, string Channel, string ThreadTs) : IRequest;

public class HandleAppMentionCommandHandler : IRequestHandler<HandleAppMentionCommand>
{
    private static readonly CultureInfo ParserCulture = CultureInfo.InvariantCulture;

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
            var parseResult = await _mentionParserService.ParseMentionAsync(request.Text, cancellationToken);

            if (!parseResult.Success)
            {
                var suggestion = !string.IsNullOrWhiteSpace(parseResult.Suggestion)
                    ? parseResult.Suggestion
                    : "Cu phap chua dung. Vi du: @OopsAI Summarize last 2 hours";

                await _slackChatService.SendMessageAsync(request.Channel, suggestion, request.ThreadTs);
                return;
            }

            if (!Enum.TryParse(parseResult.Intent, true, out IntentType intent))
            {
                await _slackChatService.SendMessageAsync(
                    request.Channel,
                    $"Intent '{parseResult.Intent}' chua duoc ho tro.",
                    request.ThreadTs);
                return;
            }

            if (parseResult.Range is null ||
                string.IsNullOrWhiteSpace(parseResult.Range.Start) ||
                string.IsNullOrWhiteSpace(parseResult.Range.End))
            {
                var message = parseResult.Suggestion ?? "Khong tim thay khoang thoi gian hop le. Thu lai voi thoi gian cu the.";
                await _slackChatService.SendMessageAsync(request.Channel, message, request.ThreadTs);
                return;
            }

            if (!DateTimeOffset.TryParse(parseResult.Range.Start, ParserCulture, DateTimeStyles.RoundtripKind, out var start) ||
                !DateTimeOffset.TryParse(parseResult.Range.End, ParserCulture, DateTimeStyles.RoundtripKind, out var end))
            {
                await _slackChatService.SendMessageAsync(
                    request.Channel,
                    "Khong the doc du lieu thoi gian. Vui long thu lai sau.",
                    request.ThreadTs);
                return;
            }

            if (end <= start)
            {
                var message = parseResult.Suggestion ?? "Khoang thoi gian khong hop le. Vi du: @OopsAI Analyze from 8h to 10h";
                await _slackChatService.SendMessageAsync(request.Channel, message, request.ThreadTs);
                return;
            }

            if (!Enum.TryParse(parseResult.Source, true, out SourceType source))
            {
                source = SourceType.Datadog;
            }

            var timeRange = new TimeRange(start, end);

            var logs = await _logSourceService.GetLogsAsync(timeRange, source);

            var summary = await _summarizerService.SummarizeAsync(logs, DesiredOutputType.Text);

            await _slackChatService.SendMessageAsync(request.Channel, summary, request.ThreadTs);
        }
        catch
        {
            await _slackChatService.SendMessageAsync(
                request.Channel,
                "An unexpected error occurred. Please try again later.",
                request.ThreadTs);
        }
    }
}
