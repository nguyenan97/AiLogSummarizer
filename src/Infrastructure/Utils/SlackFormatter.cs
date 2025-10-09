using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Shared;
using System.Text;

namespace Infrastructure.Utils
{
    public static class SlackFormatter
    {
        public static string Format(SummarizerResponse response)
        {
            var sb = new StringBuilder();

            switch (response.IntentType)
            {
                case IntentType.Summarize:
                    FormatSummarize(sb, response);
                    break;

                case IntentType.Analyze:
                    FormatAnalyze(sb, response);
                    break;

                case IntentType.Report:
                    FormatReport(sb, response);
                    break;
            }

            return sb.ToString();
        }

        private static void FormatSummarize(StringBuilder sb, SummarizerResponse response)
        {
            sb.AppendLine($"📂 *Log Summary* ({response.DateRange})");
            sb.AppendLine();

            if (response.Summarize == null || response.Summarize.Count == 0)
            {
                sb.AppendLine("_Không có lỗi nào được phát hiện._");
                return;
            }

            int idx = 1;
            foreach (var s in response.Summarize)
            {
                sb.AppendLine($"{idx}. ❗ *{s.Error}* — {s.Count} lần");
                sb.AppendLine($"   - Root cause: {s.RootCause}");
                sb.AppendLine($"   - Fix: {s.FixSuggestion}");
                if (s.NextChecks.Any())
                    sb.AppendLine($"   - Next: {string.Join(", ", s.NextChecks)}");
                sb.AppendLine();
                idx++;
            }
        }

        private static void FormatAnalyze(StringBuilder sb, SummarizerResponse response)
        {
            if (response.Analyze == null)
            {
                sb.AppendLine("_Không có dữ liệu phân tích._");
                return;
            }

            var a = response.Analyze;
            sb.AppendLine($"🔍 *Chi tiết phân tích lỗi* ({response.DateRange})");
            sb.AppendLine();
            sb.AppendLine($"*Error:* {a.Error}");
            sb.AppendLine($"*Root cause:* {a.RootCause}");

            if (a.Evidence.Any())
            {
                sb.AppendLine("*Evidence:*");
                foreach (var e in a.Evidence)
                    sb.AppendLine($"```{e}```");
            }

            sb.AppendLine($"*Fix suggestion:* {a.FixSuggestion}");

            if (a.NextChecks.Any())
            {
                sb.AppendLine("*Next checks:*");
                foreach (var check in a.NextChecks)
                    sb.AppendLine($"- {check}");
            }
        }

        private static void FormatReport(StringBuilder sb, SummarizerResponse response)
        {
            if (response.Report == null)
            {
                sb.AppendLine("_Không có báo cáo._");
                return;
            }

            var r = response.Report;
            sb.AppendLine($"🗓️ *Daily Report* ({response.DateRange})");
            sb.AppendLine($"*{r.Title}*");
            sb.AppendLine();

            if (r.Errors == null || r.Errors.Count == 0)
            {
                sb.AppendLine("_Không có lỗi nào hôm nay 🎉_");
                return;
            }

            int idx = 1;
            foreach (var e in r.Errors)
            {
                sb.AppendLine($"{idx}. {e.Emoji} {e.Error} — {e.Count} lần");
                idx++;
            }
        }
    }
}
