namespace Domain.Models
{
    public class ErrorItem
    {
        public string Error { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Emoji { get; set; } = string.Empty;
    }
}
