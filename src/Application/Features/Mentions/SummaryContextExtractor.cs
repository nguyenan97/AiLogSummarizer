using System.Text.RegularExpressions;
using Domain.Models;

namespace Application.Features.Mentions
{
    public class SummaryContextExtractor
    {
        private readonly string _originalMessageText;

        // Regex để tìm một số trong yêu cầu của người dùng, ví dụ: "phân tích lỗi số 3" -> trích xuất "3"
        private static readonly Regex IndexParserRegex = new Regex(@"(?:lỗi số|error number|error|số|number)\s+(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Regex để tìm một khối lỗi dựa vào chỉ số của nó, ví dụ: tìm khối bắt đầu bằng "3. ❗ `System.InvalidCastException`"
        // Nó sẽ khớp cả dòng tiêu đề và toàn bộ nội dung cho đến khi gặp dấu --- hoặc hết chuỗi.
        private static readonly Regex ErrorBlockRegex = new Regex(@"^\s*\*{0,1}(\d+)\.\*?\s*:exclamation:\s*`([^`]+)`.*?(?=^\s*---|\z)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline);

        // Regex để tìm khoảng thời gian trong tin nhắn gốc
        private static readonly Regex DateRangeRegex = new Regex(@"\(([\d\-TZ:.]+\s*→\s*[\d\-TZ:.]+)\)", RegexOptions.Compiled);

        public SummaryContextExtractor(string originalMessageText)
        {
            _originalMessageText = originalMessageText ?? string.Empty;
        }

        /// <summary>
        /// Phân tích văn bản trả lời của người dùng để lấy ra chỉ số lỗi.
        /// </summary>
        /// <param name="replyText">Ví dụ: "phân tích chi tiết lỗi số 3"</param>
        /// <returns>Số 3, hoặc 0 nếu không tìm thấy.</returns>
        public int ParseErrorIndex(string replyText)
        {
            if (string.IsNullOrWhiteSpace(replyText))
            {
                return 0;
            }

            var match = IndexParserRegex.Match(replyText);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            {
                return index;
            }

            return 0;
        }

        /// <summary>
        /// Lấy chuỗi định danh lỗi (tên exception) từ tin nhắn gốc dựa vào chỉ số.
        /// </summary>
        /// <param name="index">Chỉ số lỗi (ví dụ: 3).</param>
        /// <returns>Chuỗi "System.InvalidCastException...", hoặc null nếu không tìm thấy.</returns>
        public string? GetErrorIdentifierByIndex(int index)
        {
            if (string.IsNullOrWhiteSpace(_originalMessageText) || index <= 0)
            {
                return null;
            }

            // Tìm tất cả các khối lỗi trong tin nhắn gốc
            var matches = ErrorBlockRegex.Matches(_originalMessageText);

            foreach (Match match in matches)
            {
                // `match.Groups[1]` là chỉ số lỗi (ví dụ: "1", "2", "3")
                if (int.TryParse(match.Groups[1].Value, out int foundIndex) && foundIndex == index)
                {
                    // `match.Groups[2]` là tên exception bên trong dấu ``
                    return match.Groups[2].Value.Trim();
                }
            }

            return null; // Không tìm thấy lỗi với chỉ số đó
        }

        /// <summary>
        /// Trích xuất khoảng thời gian log từ tin nhắn gốc.
        /// </summary>
        /// <returns>Một đối tượng chứa thời gian bắt đầu và kết thúc.</returns>
        public LogQueryContext? ExtractLogContext()
        {
            if (string.IsNullOrWhiteSpace(_originalMessageText))
            {
                return null;
            }

            var match = DateRangeRegex.Match(_originalMessageText);
            if (match.Success)
            {
                var parts = match.Groups[1].Value.Split('→');
                if (parts.Length == 2)
                {
                    return new LogQueryContext
                    {
                        From = parts[0].Trim(),
                        To = parts[1].Trim()
                        // Các thuộc tính khác như Limit có thể được đặt mặc định
                    };
                }
            }

            return null;
        }
    }
}
