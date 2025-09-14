using AiLogSummarizer.Api.Models;
using AiLogSummarizer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiLogSummarizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummariesController : ControllerBase
{
    private readonly ILogSummarizer _summarizer;
    private readonly SlackNotifier _notifier;

    public SummariesController(ILogSummarizer summarizer, SlackNotifier notifier)
    {
        _summarizer = summarizer;
        _notifier = notifier;
    }

    [HttpPost]
    public async Task<ActionResult<LogSummary>> Post(SummarizeRequest request)
    {
        var result = await _summarizer.SummarizeAsync(request);
        await _notifier.NotifyAsync(result);
        return Ok(result);
    }
}
