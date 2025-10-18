using System.Security.Cryptography;
using System.Text;
using Domain.Models;
using Domain.Shared;

namespace Infrastructure.Utils
{
    public static class ErrorCentricLogChunker
    {
        /// <summary>
        /// Tạo các chunk log dựa trên nguyên tắc "một lỗi, một chunk".
        /// </summary>
        /// <param name="sanitizedLogs">Danh sách log ĐÃ ĐƯỢC LÀM SẠCH bởi LogSanitizer.</param>
        /// <param name="contextWindow">Số lượng dòng log (không phải lỗi) trước dòng lỗi để lấy làm ngữ cảnh.</param>
        /// <returns>Một danh sách các LogChunk, mỗi chunk chứa một vấn đề duy nhất.</returns>
        public static List<LogChunk> ChunkLogs(List<TraceLog> sanitizedLogs, int contextWindow = 7)
        {
            if (sanitizedLogs == null || !sanitizedLogs.Any())
            {
                return new List<LogChunk>();
            }

            var chunks = new List<LogChunk>();

            for (int i = 0; i < sanitizedLogs.Count; i++)
            {
                var currentLog = sanitizedLogs[i];

                // Chỉ bắt đầu một chunk mới khi gặp một dòng log lỗi
                if (currentLog.Level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    var chunkLogs = new List<TraceLog>();

                    // 1. Lấy các dòng ngữ cảnh phía trước
                    // Duyệt ngược từ dòng trước dòng lỗi để tìm 'contextWindow' dòng không phải lỗi
                    int contextLinesFound = 0;
                    for (int j = i - 1; j >= 0 && contextLinesFound < contextWindow; j--)
                    {
                        // Chỉ thêm các dòng không phải lỗi vào ngữ cảnh để tránh nhiễu
                        if (!sanitizedLogs[j].Level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                        {
                            chunkLogs.Insert(0, sanitizedLogs[j]); // Insert vào đầu để giữ đúng thứ tự
                            contextLinesFound++;
                        }
                    }

                    // 2. Thêm chính dòng lỗi đó vào
                    chunkLogs.Add(currentLog);

                    // 3. Tạo đối tượng LogChunk
                    var newChunk = new LogChunk
                    {
                        Logs = chunkLogs,
                        OriginalStartIndex = i - contextLinesFound,
                        ContainsError = true,
                        FirstTimestamp = chunkLogs.First().Timestamp
                    };

                    newChunk.Fingerprint = GenerateFingerprint(newChunk);
                    chunks.Add(newChunk);
                }
            }

            // 4. Chống trùng lặp: Giữ lại chỉ một chunk cho mỗi lỗi giống hệt nhau
            var uniqueChunks = chunks
                .GroupBy(c => c.Fingerprint)
                .Select(g => g.First()) // Lấy chunk đầu tiên đại diện cho nhóm
                .OrderBy(c => c.OriginalStartIndex)
                .ToList();

            return uniqueChunks;
        }

        /// <summary>
        /// Tạo một "dấu vân tay" duy nhất cho một chunk dựa trên nội dung lỗi.
        /// </summary>
        private static string GenerateFingerprint(LogChunk chunk)
        {
            // Chỉ lấy các dòng lỗi để tạo fingerprint, vì ngữ cảnh có thể khác nhau
            var errorMessages = chunk.Logs
                .Where(l => l.Level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                .Select(l => l.Message); // l.Message đã được làm sạch và cắt bớt

            if (!errorMessages.Any()) return string.Empty;

            var combined = string.Join("\n", errorMessages);

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
