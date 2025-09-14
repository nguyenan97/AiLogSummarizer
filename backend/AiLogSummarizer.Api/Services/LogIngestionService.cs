using AiLogSummarizer.Api.Models;
using Microsoft.AspNetCore.Http;

namespace AiLogSummarizer.Api.Services;

public class LogIngestionService : ILogIngestionService
{
    private readonly IChunker _chunker;
    public LogIngestionService(IChunker chunker) => _chunker = chunker;

    public async Task<IEnumerable<LogChunk>> IngestAsync(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        return _chunker.Chunk(content, 4000);
    }
}
