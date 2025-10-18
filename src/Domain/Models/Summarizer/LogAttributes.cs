using System.Text.Json.Serialization;

namespace Domain.Models.Summarizer
{
    public class LogAttributes
    {
        [JsonPropertyName("attributes")]
        public InnerAttributes? InnerAttributes { get; set; }
    }
}
