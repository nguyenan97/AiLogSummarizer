using AiLogSummarizer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiLogSummarizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly ILogIngestionService _ingestion;
    public LogsController(ILogIngestionService ingestion) => _ingestion = ingestion;

    [HttpPost("upload")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<ActionResult<object>> Upload(IFormFile file)
    {
        var chunks = (await _ingestion.IngestAsync(file)).ToList();
        var preview = chunks.FirstOrDefault()?.Content ?? string.Empty;
        return Ok(new { count = chunks.Count, preview });
    }
}
