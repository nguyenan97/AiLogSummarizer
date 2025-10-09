using Domain.Models;
using Domain.Shared;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Services.LogSources
{
    public static class LogChunker
    {
        // Regex patterns
        private static readonly Regex TraceIdPattern = new(@"\b(?:TraceID|trace_id|correlation_id)[:=\s]*([a-zA-Z0-9\-_]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ErrorPattern = new(@"\b(ERROR|FATAL|Exception|Failed|Timeout|NullReference|Stack trace|Caused by)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ServicePattern = new(@"```math
(?<service>[a-zA-Z0-9]+)(?:Service)?```", RegexOptions.Compiled);
        private static readonly Regex TimestampPattern = new(@"\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}(?:\.\d+)?Z?", RegexOptions.Compiled);

        /// <summary>
        /// Chunk logs using hybrid strategy: TraceID > ErrorBoundary > Service+TimeWindow
        /// </summary>
        public static List<LogChunk> ChunkLogs(IEnumerable<TraceLog> logs, int contextWindow = 15, TimeSpan timeWindow = default)
        {
            var logList = logs.ToList();
            if (!logList.Any()) return new();

            if (timeWindow == default) timeWindow = TimeSpan.FromMinutes(5);

            var chunks = new ConcurrentBag<LogChunk>();
            var processedTraceIds = new HashSet<string>();
            var errorIndices = new List<int>();

            // Step 1: Group theo TraceId
            var traceGroups = new Dictionary<string, List<(int Index, TraceLog Log)>>();
            for (int i = 0; i < logList.Count; i++)
            {
                var log = logList[i];
                var traceId = ExtractTraceId(log.Message);
                if (!string.IsNullOrEmpty(traceId))
                {
                    if (!traceGroups.ContainsKey(traceId))
                        traceGroups[traceId] = new();
                    traceGroups[traceId].Add((i, log));
                }

                if (IsErrorLine(log.Message))
                    errorIndices.Add(i);
            }

            // Step 2: Build chunks theo TraceId
            foreach (var group in traceGroups)
            {
                if (processedTraceIds.Contains(group.Key)) continue;

                var firstIndex = group.Value.First().Index;
                var lines = group.Value.Select(x => x.Log).ToList();

                var chunk = BuildChunkFromLogs(lines, logList, firstIndex);
                chunk.TraceId = group.Key;
                chunk.Fingerprint = GenerateFingerprint(chunk.Logs);
                chunks.Add(chunk);

                processedTraceIds.Add(group.Key);
            }

            // Step 3: Chunk theo error boundary cho log không có TraceId
            Parallel.ForEach(errorIndices, errorIndex =>
            {
                var log = logList[errorIndex];
                if (!string.IsNullOrEmpty(ExtractTraceId(log.Message))) return;

                var start = Math.Max(0, errorIndex - contextWindow);
                var end = Math.Min(logList.Count, errorIndex + contextWindow + 1);

                var lines = logList.GetRange(start, end - start);
                var chunk = BuildChunkFromLogs(lines, logList, start);
                if (chunk != null)
                {
                    chunk.Fingerprint = GenerateFingerprint(chunk.Logs);
                    chunks.Add(chunk);
                }
            });

            // Step 4 (Optional): Fallback - Chunk by Service + Time Window
            // Chỉ dùng nếu cần bao phủ toàn bộ log (ít dùng trong AI summarizer vì tốn cost)
            // Bạn có thể bật tính năng này nếu muốn phân tích toàn cảnh, không chỉ lỗi.

            // Step 5: Deduplicate by fingerprint
            var uniqueChunks = chunks
                .GroupBy(c => c.Fingerprint)
                .Select(g => g.First())
                .OrderBy(c => c.OriginalStartIndex)
                .ToList();

            return uniqueChunks;
        }

        private static LogChunk BuildChunkFromLogs(List<TraceLog> chunkLogs, List<TraceLog> allLogs, int originalStartIndex)
        {
            var chunk = new LogChunk
            {
                Logs = chunkLogs,
                OriginalStartIndex = originalStartIndex,
                ContainsError = chunkLogs.Any(l => IsErrorLine(l.Message))
            };

            // metadata
            foreach (var log in chunkLogs.Take(3))
            {
                if (string.IsNullOrEmpty(chunk.ServiceName))
                    chunk.ServiceName = ExtractServiceName(log.Message);

                if (chunk.FirstTimestamp == null)
                    chunk.FirstTimestamp = log.Timestamp;
            }

            return chunk;
        }

        private static string? ExtractTraceId(string message)
        {
            var match = TraceIdPattern.Match(message);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static bool IsErrorLine(string message)
        {
            return ErrorPattern.IsMatch(message);
        }

        private static string? ExtractServiceName(string message)
        {
            var match = ServicePattern.Match(message);
            return match.Success ? match.Groups["service"].Value : null;
        }

        private static DateTime? ExtractTimestamp(string message)
        {
            var match = TimestampPattern.Match(message);
            if (match.Success && DateTime.TryParse(match.Value, out DateTime dt))
                return dt;
            return null;
        }

        private static string GenerateFingerprint(List<TraceLog> logs)
        {
            using var sha256 = SHA256.Create();
            var combined = string.Join("\n", logs.Select(l => l.Message));
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
