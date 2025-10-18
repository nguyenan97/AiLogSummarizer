using System.Text.Json.Serialization;

namespace Domain.Models.Summarizer
{
    public class InnerAttributes
    {
        [JsonPropertyName("Exception")]
        public string? Exception { get; set; }
    }
}
