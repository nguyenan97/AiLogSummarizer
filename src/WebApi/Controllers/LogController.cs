using Application.Interfaces;
using Domain.Models;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly ICompositeLogSource _compositeLogSource;
    private readonly IFakeLogGenerateService _fakeLogGenerateService;

    public LogController(ICompositeLogSource compositeLogSource, IFakeLogGenerateService fakeLogGenerateService)
    {
        _compositeLogSource = compositeLogSource;
        _fakeLogGenerateService = fakeLogGenerateService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchLog([FromBody] LogQueryContext model)
    {
        model.Source = Domain.Shared.SourceType.Datadog;
        var result = await _compositeLogSource.GetLogsAsync(model);
        return Ok(result);
    }

    [HttpPost("generate-fake")]
    public async Task<IActionResult> GenerateFakeLog([FromQuery] string language, [FromQuery] string severity)
    {
        var log = await _fakeLogGenerateService.GenerateFakeLog(language, severity);
        Serilog.Log.Error(log.Message, log);
        return Ok(log);
    }
}