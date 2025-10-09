namespace Application.Models
{
    public class BatchSummarizeResponseItem
    {
        public string ChunkId { get; set; } = string.Empty;
        public SummarizerResponse? Summary { get; set; }
    }
}
