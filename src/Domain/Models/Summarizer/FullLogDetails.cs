using System.Text.Json.Serialization;

namespace Domain.Models.Summarizer
{
    public class FullLogDetails
    {
        [JsonPropertyName("attributes")]
        public LogAttributes? Attributes { get; set; }
    }
}
