namespace Domain.Models
{
    public class AnalyzeResult
    {
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Root cause phân tích kỹ hơn (có thể dài hơn bản Summarize).
        /// </summary>
        public string RootCause { get; set; } = string.Empty;

        /// <summary>
        /// Stacktrace hoặc các log raw liên quan (nếu có).
        /// </summary>
        public List<string> Evidence { get; set; } = new();

        /// <summary>
        /// Giải pháp fix đề xuất (chi tiết hơn SummarizeResult).
        /// </summary>
        public string FixSuggestion { get; set; } = string.Empty;

        /// <summary>
        /// Các bước kiểm tra tiếp theo hoặc đề xuất monitor.
        /// </summary>
        public List<string> NextChecks { get; set; } = new();
    }
}
