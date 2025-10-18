namespace Domain.Models
{
    public class BatchSummarizeRequestItem
    {
        // Dùng một ID để map kết quả trả về với chunk ban đầu
        public string ChunkId { get; set; } = string.Empty;
        public string LogChunk { get; set; } = string.Empty;
    }
}
