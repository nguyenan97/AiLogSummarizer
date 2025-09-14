using AiLogSummarizer.Api.Models;

namespace AiLogSummarizer.Api.Services;

public interface ILogSummarizer
{
    Task<LogSummary> SummarizeAsync(SummarizeRequest request);
}
