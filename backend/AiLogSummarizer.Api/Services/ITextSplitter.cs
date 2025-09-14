namespace AiLogSummarizer.Api.Services;

public interface ITextSplitter
{
    IEnumerable<string> Split(string text, int chunkSize);
}
