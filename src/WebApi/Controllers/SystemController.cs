using System.Text.Json;
using Application.Common.Queries;
using Application.Features.Mentions;
using Application.Interfaces;
using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    // This controller is now deprecated. All endpoints have been moved to specialized controllers:
    // - DateTimeController: for datetime operations
    // - LogController: for log search and fake log generation
    // - MentionController: for mention/chat testing
    // - SummarizerController: for summarization testing
    // TODO: Remove this controller after verifying all endpoints work in new controllers.

    private readonly IMediator _mediator;
    private readonly ICompositeLogSource _compositeLogSource;
    private readonly ISummarizerService _summarizerService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFakeLogGenerateService _fakeLogGenerateService;

    public SystemController(IMediator mediator, ICompositeLogSource compositeLogSource,
        IWebHostEnvironment webHostEnvironment,
        ISummarizerService summarizerService,
        IFakeLogGenerateService fakeLogGenerateService)
    {
        _mediator = mediator;
        _compositeLogSource = compositeLogSource;
        _webHostEnvironment = webHostEnvironment;
        _summarizerService = summarizerService;
        _fakeLogGenerateService = fakeLogGenerateService;
    }

    [Obsolete("Use DateTimeController.GetDateTime instead")]
    [HttpGet("datetime")]
    public async Task<IActionResult> GetDateTime()
    {
        var result = await _mediator.Send(new GetDateNowQuery());
        return Ok(result);
    }

    [Obsolete("Use LogController.SearchLog instead")]
    [HttpPost("search-logs")]
    public async Task<IActionResult> SearchLog([FromBody]LogQueryContext model)
    {
        model.Source = Domain.Shared.SourceType.Datadog;
        var result = await _compositeLogSource.GetLogsAsync(model);
        return Ok(result);
    }

    [Obsolete("Use MentionController.TestChat instead")]
    [HttpPost("test-chat")]
    public async Task<IActionResult> TestChat([FromBody] HandleAppMentionCommand model)
    {
        await _mediator.Send(model);
        return Ok();
    }

    [Obsolete("Use LogController.GenerateFakeLog instead")]
    [HttpPost("generate-fake-log")]
    public async Task<IActionResult> GenerateFakeLog(string language,string severity)
    {
        var log= await _fakeLogGenerateService.GenerateFakeLog(language, severity);
        Log.Error(log.Message, log);
        return Ok(log);
    }

    [Obsolete("Use SummarizerController.TestSummarize instead")]
    [HttpPost("test-summarize")]
    public async Task<ActionResult<SummarizerResponse>> Summarize()
    {
        // pick one mock log file from Tests/MockData
        string fileName = "mockData/logfile.txt";
        string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, fileName);

        string rawLogs = System.IO.File.ReadAllText(filePath);

        var traceLogs = JsonSerializer.Deserialize<List<TraceLog>>(rawLogs)?? new List<TraceLog>();

        var result = await _summarizerService.ProcessLogsAsync(traceLogs, IntentType.Summarize);
        return Ok(result);
    }
}