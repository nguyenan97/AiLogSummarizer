using System.Text;
using System.Text.Json;
using Domain.Models.Summarizer;
using Domain.Shared;

namespace Infrastructure.Utils
{
    public static class LogSanitizer
    {
        // TODO: Add AI-powered log parsing for unstructured formats
        // Next step: Integrate ML model to detect and extract structured data from plain text logs
        // Improvement: Support dynamic templates for various log formats (ELK, Splunk, custom APIs)
        public static List<TraceLog> SanitizeLogs(List<TraceLog> logContent)
        {
            var sanitizedLogs = new List<TraceLog>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    
            if (logContent == null) return sanitizedLogs;
    
            foreach (var log in logContent)
            {
                string exceptionDetails = string.Empty;
                if (!string.IsNullOrEmpty(log.FullLog))
                {
                    try
                    {
                        var fullLogDetails = JsonSerializer.Deserialize<FullLogDetails>(log.FullLog, options);
                        exceptionDetails = fullLogDetails?.Attributes?.InnerAttributes?.Exception ?? string.Empty;
    
                        //Cắt bớt log
                        exceptionDetails = TruncateStackTrace(exceptionDetails);
                    }
                    catch
                    {
                        // Bỏ qua nếu không parse được FullLog
                    }
                }

                // TẠO RA MỘT THÔNG ĐIỆP MỚI, SẠCH VÀ ĐẦY ĐỦ
                // Kết hợp thông điệp gốc và chi tiết exception
                var cleanMessage = new StringBuilder(log.Message);
                if (!string.IsNullOrEmpty(exceptionDetails))
                {
                    cleanMessage.AppendLine("\n--- Exception Details ---");
                    cleanMessage.AppendLine(exceptionDetails);
                }

                sanitizedLogs.Add(new TraceLog(
                    log.Timestamp,
                    cleanMessage.ToString(),
                    log.Level,
                    log.Source,
                    string.Empty,
                    string.Empty
                ));
            }

            return sanitizedLogs;
        }

        private static string TruncateStackTrace(string fullStackTrace, int maxLines = 20, int linesFromTop = 10)
        {
            if (string.IsNullOrEmpty(fullStackTrace)) return string.Empty;

            var lines = fullStackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= maxLines)
            {
                return fullStackTrace;
            }

            var truncatedLines = new List<string>();

            // Lấy các dòng quan trọng ở đầu
            truncatedLines.AddRange(lines.Take(linesFromTop));
            truncatedLines.Add("\n... [Stack trace truncated] ...\n");

            // Tìm và giữ lại các dòng "inner exception" quan trọng
            for (int i = linesFromTop; i < lines.Length; i++)
            {
                if (lines[i].Contains("--- End of inner exception stack trace ---"))
                {
                    // Lấy dòng trước và sau nó
                    if (i > 0) truncatedLines.Add(lines[i - 1]);
                    truncatedLines.Add(lines[i]);
                    if (i + 1 < lines.Length) truncatedLines.Add(lines[i + 1]);
                }
            }

            return string.Join("\n", truncatedLines);
        }
    }
}
