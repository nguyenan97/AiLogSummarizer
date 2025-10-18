using Application.Models;
using Domain.MentionParsing.Models;
using Domain.Shared;
using System.Text;

namespace Infrastructure.Utils
{
    public static class SlackFormatter
    {
        public static string Format(SummarizerResponse response, InputLanguage language)
        {
            if (response == null)
            {
                return language == InputLanguage.Vietnamese 
                    ? ":warning: Lỗi: Nhận được phản hồi null." 
                    : ":warning: Error: Received a null response.";
            }

            // Dùng StringBuilder để xây dựng chuỗi hiệu quả
            var sb = new StringBuilder();

            switch (response.IntentType)
            {
                case IntentType.Summarize:
                    return FormatSummary(response, sb, language);

                case IntentType.Report:
                    // Thêm logic format cho Report nếu cần
                    return FormatReport(response, sb, language);

                case IntentType.Analyze:
                    // Thêm logic format cho Analyze nếu cần
                    return FormatAnalysis(response, sb, language);

                default:
                    return response.RawMarkdown; // Trả về mặc định nếu không có định dạng
            }
        }

        private static string FormatSummary(SummarizerResponse response, StringBuilder sb, InputLanguage language)
        {
            // --- Header ---
            var headerText = language == InputLanguage.Vietnamese ? "Tóm tắt Log" : "Log Summary";
            sb.AppendLine($"*:file_folder: {headerText}*");
            sb.AppendLine($"_{response.DateRange}_");
            sb.AppendLine();

            if (response.Summarize == null || !response.Summarize.Any())
            {
                var noErrorsText = language == InputLanguage.Vietnamese 
                    ? ":white_check_mark: Không tìm thấy lỗi đáng kể nào." 
                    : ":white_check_mark: No significant errors found.";
                sb.AppendLine(noErrorsText);
                return sb.ToString();
            }

            for (int i = 0; i < response.Summarize.Count; i++)
            {
                var summary = response.Summarize[i];
                if (i > 0)
                {
                    sb.AppendLine("\n---");
                }

                // --- Tiêu đề lỗi (đã làm sạch) ---
                var countText = language == InputLanguage.Vietnamese ? "lần" : "times";
                sb.AppendLine($"*{i + 1}.* :exclamation: `{summary.Error}` — *{summary.Count} {countText}*");

                // --- Chi tiết lỗi trong blockquote với khoảng thở ---
                var rootCauseLabel = language == InputLanguage.Vietnamese ? "Nguyên nhân gốc rễ:" : "Root Cause:";
                var fixSuggestionLabel = language == InputLanguage.Vietnamese ? "Đề xuất sửa lỗi:" : "Suggested Fix:";
                var nextChecksLabel = language == InputLanguage.Vietnamese ? "Các bước kiểm tra tiếp theo:" : "Next Steps to Check:";

                sb.AppendLine($"> :mag: *{rootCauseLabel}* {summary.RootCause}");
                sb.AppendLine(">"); // Dòng trống để tạo khoảng thở
                sb.AppendLine($"> :wrench: *{fixSuggestionLabel}* {summary.FixSuggestion}");

                if (summary.NextChecks != null && summary.NextChecks.Any())
                {
                    sb.AppendLine(">"); // Dòng trống
                    sb.AppendLine($"> :eyes: *{nextChecksLabel}*");
                    foreach (var check in summary.NextChecks)
                    {
                        // Dùng ký tự bullet • và thụt đầu dòng để tạo danh sách rõ ràng
                        sb.AppendLine($">    • {check}");
                    }
                }
            }

            return sb.ToString();
        }

        private static string FormatReport(SummarizerResponse response, StringBuilder sb, InputLanguage language)
        {
            var missingDataText = language == InputLanguage.Vietnamese 
                ? ":warning: Dữ liệu báo cáo bị thiếu." 
                : ":warning: Report data is missing.";
            
            if (response.Report == null) return missingDataText;

            sb.AppendLine($"*{response.Report.Title}*");
            sb.AppendLine($"_({response.DateRange})_");
            sb.AppendLine();

            if (!response.Report.Errors.Any())
            {
                var normalOperationText = language == InputLanguage.Vietnamese 
                    ? ":sparkles: Hệ thống hoạt động bình thường. Không có lỗi nào được báo cáo." 
                    : ":sparkles: System operated normally. No errors reported.";
                sb.AppendLine(normalOperationText);
                return sb.ToString();
            }

            var occurrencesText = language == InputLanguage.Vietnamese ? "lần xuất hiện" : "occurrences";
            foreach (var error in response.Report.Errors)
            {
                sb.AppendLine($"{error.Emoji} *`{error.Error}`* ({error.Count} {occurrencesText})");
            }

            return sb.ToString();
        }

        private static string FormatAnalysis(SummarizerResponse response, StringBuilder sb, InputLanguage language)
        {
            var missingDataText = language == InputLanguage.Vietnamese 
                ? ":warning: Dữ liệu phân tích bị thiếu." 
                : ":warning: Analysis data is missing.";
            
            if (response.Analyze == null) return missingDataText;

            var analysis = response.Analyze;
            var analysisTitle = language == InputLanguage.Vietnamese ? "Phân tích Chi tiết" : "Deep Dive Analysis";
            var primaryIssueLabel = language == InputLanguage.Vietnamese ? "Vấn đề Chính:" : "Primary Issue:";
            var rootCauseLabel = language == InputLanguage.Vietnamese ? "Nguyên nhân Gốc rễ:" : "Root Cause:";
            var evidenceLabel = language == InputLanguage.Vietnamese ? "Bằng chứng:" : "Evidence:";
            var suggestedFixLabel = language == InputLanguage.Vietnamese ? "Đề xuất Sửa lỗi:" : "Suggested Fix:";

            sb.AppendLine($"*:sleuth_or_spy: {analysisTitle}*");
            sb.AppendLine($"_({response.DateRange})_");
            sb.AppendLine("\n---");

            sb.AppendLine($"*:rotating_light: {primaryIssueLabel}* `{analysis.Error}`");
            sb.AppendLine();
            sb.AppendLine($">*:mag: {rootCauseLabel}* {analysis.RootCause}");
            sb.AppendLine();
            sb.AppendLine($">*:scroll: {evidenceLabel}*");
            foreach (var evidence in analysis.Evidence)
            {
                sb.AppendLine($"> ```{evidence}```");
            }
            sb.AppendLine();
            sb.AppendLine($">*:wrench: {suggestedFixLabel}* {analysis.FixSuggestion}");

            return sb.ToString();
        }
    }
}
