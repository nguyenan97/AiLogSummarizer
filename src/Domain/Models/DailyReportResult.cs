namespace Domain.Models
{
    public class DailyReportResult
    {
        public string Title { get; set; } = string.Empty;
        public List<ErrorItem> Errors { get; set; } = new();
    }
}
