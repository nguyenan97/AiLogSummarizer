using MediatR;
using Application.Interfaces;

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
        // Send immediate response
        await _slackChatService.SendMessageAsync(request.Channel, "Đã nhận yêu cầu — đang xử lý…", request.ThreadTs);

        // TODO: Implement parsing, logging, summarizing, and sending final response
    }
}