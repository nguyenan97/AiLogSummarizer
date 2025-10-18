using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Services.LogSources
{
    public class DatadogLogsSearchRequest
    {
        [JsonPropertyName("filter")]
        public DatadogLogsFilter? Filter { get; set; }

        [JsonPropertyName("page")]
        public DatadogPage? Page { get; set; }

        [JsonPropertyName("sort")]
        public string? Sort { get; set; } = "desc"; // "asc" or "desc"
    }

    public class DatadogLogsFilter
    {
        /// <summary>
        /// The start time of the query. ISO 8601 format (e.g., "2025-10-11T00:00:00Z")
        /// </summary>
        [JsonPropertyName("from")]
        public string? From { get; set; }

        /// <summary>
        /// The end time of the query. ISO 8601 format.
        /// </summary>
        [JsonPropertyName("to")]
        public string? To { get; set; }

        /// <summary>
        /// The main Datadog search query string.
        /// </summary>
        [JsonPropertyName("query")]
        public string? Query { get; set; }

        /// <summary>
        /// Optional: specify indexes (e.g., "main", "custom_index").
        /// </summary>
        [JsonPropertyName("indexes")]
        public List<string>? Indexes { get; set; }

        /// <summary>
        /// Optional: restrict search to specific storage tiers (e.g., ["live", "cold"])
        /// </summary>
        [JsonPropertyName("storage_tier")]
        public string? StorageTier { get; set; }
    }

    public class DatadogPage
    {
        /// <summary>
        /// Number of results per page (default: 10)
        /// </summary>
        [JsonPropertyName("limit")]
        public int? Limit { get; set; } = 10;

        /// <summary>
        /// Cursor for pagination (from previous response.meta.page.after)
        /// </summary>
        [JsonPropertyName("cursor")]
        public string? Cursor { get; set; }
    }
}
