using Application.Common.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IMediator _mediator;

    public SystemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("datetime")]
    public async Task<IActionResult> GetDateTime()
    {
        var result = await _mediator.Send(new GetDateNowQuery());
        return Ok(result);
    }
}