namespace AiLogSummarizer.Api.Models;

public class SummarizeRequest
{
    public string Service { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string RawLog { get; set; } = string.Empty;
}
