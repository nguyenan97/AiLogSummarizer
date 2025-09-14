using AiLogSummarizer.Api.Models;

namespace AiLogSummarizer.Api.Services;

public class SimpleChunker : IChunker
{
    private readonly ITextSplitter _splitter;
    public SimpleChunker(ITextSplitter splitter) => _splitter = splitter;

    public IEnumerable<LogChunk> Chunk(string content, int chunkSize)
        => _splitter.Split(content, chunkSize)
                    .Select((c, i) => new LogChunk(i, c));
}
