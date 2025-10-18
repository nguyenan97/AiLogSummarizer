using System.Text.Json.Serialization;

namespace WebApi.Models
{
    public class RawLog
    {
        [JsonPropertyName("@t")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("@mt")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("@l")]
        public string Level { get; set; } = string.Empty;

        public string SourceContext { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public string FullLog { get; set; } = string.Empty;
    }
}
