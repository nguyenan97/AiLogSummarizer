namespace AiLogSummarizer.Api.Models;

public class LogSummary
{
    public string RootCause { get; set; } = string.Empty;
    public List<string> KeyErrors { get; set; } = new();
    public List<string> FixSuggestions { get; set; } = new();
}
