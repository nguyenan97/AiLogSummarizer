using Microsoft.AspNetCore.Mvc;

namespace AiLogSummarizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("OK");
}
