using System.Text.Json;
using Application.Interfaces;
using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummarizerController : ControllerBase
{
    private readonly ISummarizerService _summarizerService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SummarizerController(ISummarizerService summarizerService, IWebHostEnvironment webHostEnvironment)
    {
        _summarizerService = summarizerService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpPost("test")]
    public async Task<ActionResult<SummarizerResponse>> TestSummarize()
    {
        // pick one mock log file from Tests/MockData
        string fileName = "mockData/logfile.txt";
        string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, fileName);

        string rawLogs = System.IO.File.ReadAllText(filePath);

        var traceLogs = JsonSerializer.Deserialize<List<TraceLog>>(rawLogs) ?? new List<TraceLog>();

        var result = await _summarizerService.ProcessLogsAsync(traceLogs, IntentType.Summarize);
        return Ok(result);
    }
}