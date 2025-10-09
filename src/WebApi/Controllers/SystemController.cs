using Application.Common.Queries;
using Application.Features.Mentions;
using Application.Interfaces;
using Domain.Models;
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

    [HttpPost("search-logs")]
    public async Task<IActionResult> SearchLog([FromBody]LogQueryContext model)
    {
        model.Source = Domain.Shared.SourceType.Datadog;
        var result = await _compositeLogSource.GetLogsAsync(model);
        return Ok(result);
    }
    [HttpPost("test-chat")]
    public async Task<IActionResult> TestChat([FromBody] HandleAppMentionCommand model)
    {
        await _mediator.Send(model);
        return Ok();
    }
}