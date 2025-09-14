using AiLogSummarizer.Api.Models;

namespace AiLogSummarizer.Api.Services;

public interface IChunker
{
    IEnumerable<LogChunk> Chunk(string content, int chunkSize);
}
