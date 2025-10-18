using Application.Features.Mentions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MentionController : ControllerBase
{
    private readonly IMediator _mediator;

    public MentionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("test-chat")]
    public async Task<IActionResult> TestChat([FromBody] HandleAppMentionCommand model)
    {
        await _mediator.Send(model);
        return Ok();
    }
}