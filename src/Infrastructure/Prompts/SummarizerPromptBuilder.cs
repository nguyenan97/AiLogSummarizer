using System.Text.Json;
using Application.Models;
using Domain.Models;
using Domain.Shared;
using Infrastructure.Utils;
using Microsoft.Extensions.AI;

namespace Infrastructure.Prompts
{
    public static class SummarizerPromptBuilder
    {
        #region Prompts for Chunk Processing

        /// <summary>
        /// (CẬP NHẬT) Lấy template cho prompt xử lý lô.
        /// Chứa hướng dẫn chi tiết về persona, ngữ cảnh, và các bước thực hiện.
        /// </summary>
        public static string GetBatchChunkPromptTemplate()
        {
            // Sử dụng verbatim string (@"") để dễ đọc hơn
            return @"Bạn là một Kỹ sư Cao cấp về Độ tin cậy của Hệ thống (Senior SRE) với chuyên môn sâu về phân tích log. 
Nhiệm vụ của bạn là phân tích các đoạn log (chunk) từ một ứng dụng phân tán để xác định và tóm tắt các lỗi nghiêm trọng.

**YÊU CẦU:**
Phân tích TỪNG MỤC trong mảng JSON log đầu vào. Với mỗi mục, hãy thực hiện các bước sau:
1.  **Xác định Lỗi Chính:** Tìm ra thông điệp lỗi hoặc ngoại lệ cốt lõi. Chuẩn hóa nó bằng cách loại bỏ các định danh duy nhất (ví dụ: UUID, ID request).
2.  **Tìm Nguyên nhân Gốc rễ (Root Cause):** Dựa *chỉ* vào các dòng log được cung cấp trong chunk, suy luận ra nguyên nhân kỹ thuật khả năng cao nhất.
3.  **Đề xuất Sửa lỗi:** Đưa ra một đề xuất sửa lỗi cụ thể, có tính hành động cho đội ngũ phát triển.
4.  **Các bước Kiểm tra Tiếp theo:** Liệt kê các khu vực hoặc truy vấn mà một kỹ sư nên kiểm tra để điều tra sâu hơn.

**QUY TẮC ĐẦU RA:**
-   Kết quả trả về PHẢI là một mảng JSON duy nhất, chứa các đối tượng có cấu trúc `BatchSummarizeResponseItem`.
-   Mỗi `BatchSummarizeResponseItem` phải chứa `ChunkId` tương ứng từ input và một đối tượng `Summary` theo cấu trúc `SummarizerResponse`.
-   Nếu một chunk log không chứa lỗi nào có thể xác định, hãy trả về một đối tượng `Summary` với thuộc tính `Summarize` là một mảng rỗng (`[]`).
-   Chỉ trả lời bằng MẢNG JSON. KHÔNG thêm bất kỳ văn bản, giải thích, hay định dạng markdown nào bên ngoài mảng JSON đó.";
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt phân tích MỘT LÔ chunk log.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildBatchChunkPrompt(IEnumerable<BatchSummarizeRequestItem> batch)
        {
            var jsonInput = JsonSerializer.Serialize(batch, JsonOptions.Default);
            return new[]
            {
                new ChatMessage(ChatRole.System, GetBatchChunkPromptTemplate()),
                new ChatMessage(ChatRole.User, jsonInput)
            };
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt phân tích MỘT chunk log.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildChunkPrompt(string logChunk)
        {
            // Tái sử dụng logic của prompt xử lý lô để đảm bảo tính nhất quán
            var systemPrompt = GetBatchChunkPromptTemplate()
                .Replace("Phân tích TỪNG MỤC trong mảng JSON log đầu vào.", "Phân tích đoạn log sau đây.")
                .Replace("Trả về một mảng JSON duy nhất chứa các đối tượng có cấu trúc `BatchSummarizeResponseItem`.", "Trả về một đối tượng JSON duy nhất theo cấu trúc `SummarizerResponse` với `IntentType=Summarize`.");

            return new[]
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, logChunk)
            };
        }

        #endregion

        #region Prompts for Merging and Reporting

        /// <summary>
        /// (CẬP NHẬT) Prompt hợp nhất nhiều tóm tắt thành một.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildMergePrompt(IEnumerable<SummarizerResponse> partialSummaries)
        {
            var jsonParts = partialSummaries.Select(p => JsonSerializer.Serialize(p, JsonOptions.Default));
            var joined = string.Join("\n\n---\n\n", jsonParts);

            return new[]
            {
                new ChatMessage(ChatRole.System, @"Bạn là một AI chuyên gia về tổng hợp và tương quan log.
Nhiệm vụ của bạn là hợp nhất nhiều kết quả tóm tắt lỗi riêng lẻ thành một danh sách tổng hợp duy nhất.

**QUY TRÌNH HỢP NHẤT:**
1.  **Gom nhóm:** Kết hợp các mục có trường `Error` giống hệt nhau về mặt ngữ nghĩa.
2.  **Tổng hợp:** Khi hợp nhất, TÍNH TỔNG (SUM) các giá trị `Count`.
3.  **Chắt lọc:** Đối với `RootCause` và `FixSuggestion`, hãy tổng hợp lại thành một lời giải thích chung, súc tích và tốt nhất từ các mục được hợp nhất.
4.  **Sắp xếp:** Sắp xếp danh sách kết quả cuối cùng theo thứ tự `Count` giảm dần, lỗi nhiều nhất lên đầu.

**QUY TẮC ĐẦU RA:**
-   Kết quả trả về PHẢI là một đối tượng JSON duy nhất theo cấu trúc `SummarizerResponse` với `IntentType=Summarize`.
-   Chỉ trả lời bằng JSON, không giải thích gì thêm."),
                new ChatMessage(ChatRole.User, $"Dưới đây là các tóm tắt riêng lẻ cần được hợp nhất:\n{joined}")
            };
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt tạo báo cáo cho Slack.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildReportPrompt(IEnumerable<SummarizerResponse> partialSummaries, string dateRange)
        {
            var joined = string.Join("\n\n---\n\n", partialSummaries.Select(p => JsonSerializer.Serialize(p.Summarize, JsonOptions.Default)));

            return new[]
            {
                new ChatMessage(ChatRole.System, @"Bạn là một AI truyền thông cho đội ngũ DevOps.
Nhiệm vụ của bạn là tạo ra một báo cáo tổng kết lỗi hàng ngày, ngắn gọn, phù hợp để đăng lên kênh Slack.

**YÊU CẦU BÁO CÁO:**
1.  **Tiêu đề (Title):** Tạo một tiêu đề súc tích, phản ánh tình trạng chung của hệ thống trong khoảng thời gian được cung cấp. Ví dụ: 'Báo cáo Lỗi Hàng ngày: 3 Vấn đề Nghiêm trọng được Xác định' hoặc 'Ngày Hoạt động Ổn định: Ghi nhận một số Cảnh báo nhỏ'.
2.  **Danh sách Lỗi (Errors):** Liệt kê các lỗi hàng đầu (tối đa 5). Với mỗi lỗi, bao gồm `Error`, `Count`.
3.  **Emoji:** Gán một emoji phù hợp cho mỗi lỗi để thể hiện mức độ nghiêm trọng: 🔴 cho lỗi nghiêm trọng (số lượng lớn, ngoại lệ fatal), 🟡 cho cảnh báo, 🟢 nếu không có lỗi đáng kể.

**QUY TẮC ĐẦU RA:**
-   Kết quả trả về PHẢI là một đối tượng JSON duy nhất theo cấu trúc `SummarizerResponse` với `IntentType=Report`.
-   Chỉ trả lời bằng JSON, không giải thích gì thêm."),
                new ChatMessage(ChatRole.User, $"Khoảng thời gian: {dateRange}\nDữ liệu tóm tắt:\n{joined}")
            };
        }

        /// <summary>
        /// (CẬP NHẬT) Prompt phân tích sâu.
        /// </summary>
        public static IEnumerable<ChatMessage> BuildAnalyzePrompt(IEnumerable<TraceLog> logs, string dateRange)
        {
            var logContent = string.Join("\n", logs.Select(l => $"[{l.Timestamp:u}] [{l.Level}] {l.Source}: {l.Message}"));

            return new[]
            {
                new ChatMessage(ChatRole.System, @"Bạn là một chuyên gia điều tra sự cố và phân tích nguyên nhân gốc rễ (RCA).
Nhiệm vụ của bạn là thực hiện một phân tích sâu trên toàn bộ chuỗi log được cung cấp để xác định NGUYÊN NHÂN GỐC RỄ DUY NHẤT khả năng cao nhất gây ra sự cố chính.

**QUY TRÌNH PHÂN TÍCH:**
1.  **Xác định Sự cố chính:** Quét toàn bộ log để tìm ra điểm lỗi chính hoặc hành vi bất thường.
2.  **Truy ngược:** Từ điểm lỗi, làm việc ngược lại để tìm các sự kiện, cảnh báo, hoặc hành vi trước đó có thể là nguyên nhân.
3.  **Trích xuất Bằng chứng (Evidence):** Trích xuất các dòng log cụ thể làm bằng chứng hỗ trợ cho kết luận của bạn. Trích dẫn chúng trực tiếp.
4.  **Kết luận Nguyên nhân (RootCause):** Đưa ra một tuyên bố súc tích, duy nhất về nguyên nhân gốc rễ.
5.  **Đề xuất Sửa lỗi (FixSuggestion) và Các bước Tiếp theo (NextChecks).**

**QUY TẮC ĐẦU RA:**
-   Kết quả trả về PHẢI là một đối tượng JSON duy nhất theo cấu trúc `SummarizerResponse` với `IntentType=Analyze`.
-   Chỉ trả lời bằng JSON, không giải thích gì thêm."),
                new ChatMessage(ChatRole.User, $"Khoảng thời gian: {dateRange}\nToàn bộ log cần phân tích:\n{logContent}")
            };
        }

        #endregion
    }
}
