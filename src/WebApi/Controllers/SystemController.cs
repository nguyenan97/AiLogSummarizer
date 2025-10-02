using Application.Common.Queries;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICompositeLogSource _compositeLogSource;

    public SystemController(IMediator mediator, ICompositeLogSource compositeLogSource)
    {
        _mediator = mediator;
        _compositeLogSource = compositeLogSource;
    }

    [HttpGet("datetime")]
    public async Task<IActionResult> GetDateTime()
    {
        var result = await _mediator.Send(new GetDateNowQuery());
        return Ok(result);
    }
    [HttpGet("test-logs")]
    public async Task<IActionResult> GetLogs()
    {
        var result = await _compositeLogSource.GetLogsAsync(new Domain.Models.GetLogModel
        {
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            Source = Domain.Shared.SourceType.Datadog
        });
        return Ok(result);
    }
}