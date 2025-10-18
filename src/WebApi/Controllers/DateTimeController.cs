using Application.Common.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DateTimeController : ControllerBase
{
    private readonly IMediator _mediator;

    public DateTimeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDateTime()
    {
        var result = await _mediator.Send(new GetDateNowQuery());
        return Ok(result);
    }
}