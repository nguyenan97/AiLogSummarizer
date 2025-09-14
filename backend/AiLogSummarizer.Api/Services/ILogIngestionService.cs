using AiLogSummarizer.Api.Models;
using Microsoft.AspNetCore.Http;

namespace AiLogSummarizer.Api.Services;

public interface ILogIngestionService
{
    Task<IEnumerable<LogChunk>> IngestAsync(IFormFile file);
}
