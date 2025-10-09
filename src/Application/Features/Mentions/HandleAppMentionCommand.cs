using System.Globalization;
using Application.Interfaces;
using Domain.MentionParsing.Models;
using Domain.Shared;
using MediatR;

namespace Application.Features.Mentions;

public record HandleAppMentionCommand(string Text, string Channel, string ThreadTs) : IRequest;

public class HandleAppMentionCommandHandler : IRequestHandler<HandleAppMentionCommand>
{
    private readonly IMessageSenderService  _messageSenderService;
    private readonly IMentionParserService _mentionParserService;
    private readonly ICompositeLogSource _compositeLogSource;
    private readonly ISummarizerService _summarizerService;

    public HandleAppMentionCommandHandler(
        IMessageSenderService messageSenderService,
        IMentionParserService mentionParserService,
         ICompositeLogSource compositeLogSource,
        ISummarizerService summarizerService)
    {
        _messageSenderService = messageSenderService;
        _mentionParserService = mentionParserService;
        _compositeLogSource = compositeLogSource;
        _summarizerService = summarizerService;
    }

    public async Task Handle(HandleAppMentionCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        await _messageSenderService.SendMessageAsync(
            request.Channel,
            "Request received - processing.",
            request.ThreadTs);

        try
        {
            var parseResult = await _mentionParserService.ParseAsync(request.Text, cancellationToken);
            if (parseResult.Intent == Domain.MentionParsing.Models.IntentType.Unknown)
            {
                var unknown = parseResult.Parameters as UnknownIntentParams ?? new UnknownIntentParams();
                var unknownMessage = BuildUnknownIntentMessage(unknown);
                await _messageSenderService.SendMessageAsync(
                    request.Channel,
                    unknownMessage,
                    request.ThreadTs);
                return;

            }

            var logs = await _compositeLogSource.GetLogsAsync(new Domain.Models.LogQueryContext
            {
                From = parseResult.Parameters.Context.FromIso,
                To = parseResult.Parameters.Context.ToIso,
                Source = SourceType.Datadog,
                Limit = parseResult.Parameters.Context.Limit ?? 5
            });

            var summary = await _summarizerService.ProcessLogsAsync(logs, IntentType.Summarize, cancellationToken);

            await _messageSenderService.SendMessageAsync(request.Channel, summary.RawMarkdown, request.ThreadTs);
        }
        catch (Exception ex)
        {
            await _messageSenderService.SendMessageAsync(
                request.Channel,
                ex.Message,
                request.ThreadTs);
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine($"HandleAppMentionCommand executed in {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    private static string BuildUnknownIntentMessage(UnknownIntentParams unknown)
    {
        var lines = new List<string>
        {
            ":thinking_face: I couldn't understand the request. Try examples:",
        };

        if (unknown.Samples != null && unknown.Samples.Length > 0)
        {
            foreach (var s in unknown.Samples)
            {
                if (!string.IsNullOrWhiteSpace(s))
                    lines.Add("- " + s);
            }
        }
        else
        {
            lines.AddRange(new []
            {
                "- Latest errors for orders-api in last 2 hours",
                "- Correlate by trace 7f2c over last 2 hours (staging)",
                "- Root cause for error code 504 today",
                "- Search 'TimeoutException' over last 6 hours (production)",
            });
        }

        if (!string.IsNullOrWhiteSpace(unknown.Notes))
        {
            lines.Add("");
            lines.Add("> " + unknown.Notes);
        }

        return string.Join("\n", lines);
    }
}
