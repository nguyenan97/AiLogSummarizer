namespace AiLogSummarizer.Api.Services;

public class TextSplitter : ITextSplitter
{
    public IEnumerable<string> Split(string text, int chunkSize)
    {
        for (var i = 0; i < text.Length; i += chunkSize)
        {
            yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
        }
    }
}
