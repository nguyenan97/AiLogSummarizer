using Domain.MentionParsing.Models;
using Domain.Models;

namespace Application.Models;

public class SummarizerResponse
{
    public IntentType IntentType { get; set; }
    public List<SummarizeResult>? Summarize { get; set; }
    public AnalyzeResult? Analyze { get; set; }
    public DailyReportResult? Report { get; set; }

    public string DateRange { get; set; } = string.Empty;
    public string RawMarkdown { get; set; } = string.Empty;
}
