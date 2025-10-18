namespace Domain.Models
{
    public class SummarizeResult
    {
        public string RootCause { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;

        public int Count { get; set; }

        public string FixSuggestion { get; set; } = string.Empty;

        public List<string> NextChecks { get; set; } = new();
    }
}
