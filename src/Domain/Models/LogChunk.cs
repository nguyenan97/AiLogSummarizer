using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class LogChunk
    {
        /// <summary>
        /// Các log thuộc chunk này
        /// </summary>
        public List<TraceLog> Logs { get; set; } = new();

        /// <summary>
        /// Chỉ số bắt đầu trong log gốc (dùng cho ordering)
        /// </summary>
        public int OriginalStartIndex { get; set; }

        /// <summary>
        /// Có chứa error/exception hay không
        /// </summary>
        public bool ContainsError { get; set; }

        /// <summary>
        /// TraceId nếu có
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Service name nếu detect được
        /// </summary>
        public string? ServiceName { get; set; }

        /// <summary>
        /// Timestamp đầu tiên trong chunk
        /// </summary>
        public DateTimeOffset? FirstTimestamp { get; set; }

        /// <summary>
        /// Dùng để deduplicate (SHA256 hash của toàn bộ message trong chunk)
        /// </summary>
        public string Fingerprint { get; set; } = string.Empty;
    }
}
