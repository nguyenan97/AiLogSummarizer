using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Shared;

namespace Infrastructure.Services.LogSources
{
    public static class SimpleLogChunker
    {
        public static List<LogChunk> ChunkLogs(List<TraceLog> sanitizedLogs, int contextWindow = 5)
        {
            if (!sanitizedLogs.Any()) return new List<LogChunk>();

            var chunks = new List<LogChunk>();

            for (int i = 0; i < sanitizedLogs.Count; i++)
            {
                // Chỉ tạo chunk mới khi gặp một dòng log lỗi
                if (sanitizedLogs[i].Level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    // Lấy 'contextWindow' dòng log ngay trước đó để làm ngữ cảnh
                    // Chỉ lấy những dòng không phải là lỗi để ngữ cảnh được sạch sẽ
                    var startIndex = Math.Max(0, i - contextWindow);
                    var contextLogs = new List<TraceLog>();
                    for (int j = startIndex; j < i; j++)
                    {
                        if (!sanitizedLogs[j].Level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                        {
                            contextLogs.Add(sanitizedLogs[j]);
                        }
                    }

                    // Tạo một chunk mới chỉ chứa các dòng ngữ cảnh và dòng lỗi hiện tại
                    var newChunkLogs = new List<TraceLog>();
                    newChunkLogs.AddRange(contextLogs);
                    newChunkLogs.Add(sanitizedLogs[i]);

                    var newChunk = new LogChunk
                    {
                        Logs = newChunkLogs,
                        OriginalStartIndex = startIndex,
                        ContainsError = true,
                        // Các thuộc tính khác có thể được tính toán ở đây nếu cần
                    };

                    // Tạo fingerprint dựa trên nội dung của chunk
                    newChunk.Fingerprint = GenerateFingerprint(newChunk.Logs);
                    chunks.Add(newChunk);
                }
            }

            // Chống trùng lặp: Nếu nhiều chunk có cùng nội dung (ví dụ: cùng 1 lỗi lặp lại)
            // thì chỉ giữ lại chunk đầu tiên.
            var uniqueChunks = chunks
                .GroupBy(c => c.Fingerprint)
                .Select(g => g.First())
                .OrderBy(c => c.OriginalStartIndex)
                .ToList();

            return uniqueChunks;
        }

        private static string GenerateFingerprint(List<TraceLog> logs)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            // Chỉ hash thông điệp để xác định sự trùng lặp về nội dung
            var combined = string.Join("\n", logs.Select(l => l.Message));
            var bytes = System.Text.Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
