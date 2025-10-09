using System.Text;
using Application.Interfaces;
using Domain.Models;
using Domain.Shared;
using Humanizer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LogReader.Services.Sources
{
    public class DatadogLogSource : ILogSourceService
    {
        private readonly DatadogSettings _datadogSettings;
        public DatadogLogSource(IOptions<DatadogSettings> _datadogSettingsOption) // Fix the type name to IOptions<>
        {
            _datadogSettings = _datadogSettingsOption.Value;
        }
        public async Task<IEnumerable<TraceLog>> GetLogsAsync(LogQueryContext model)
        {
            var url = $"/api/v2/logs/events/search";
            var payload = new
            {
                filter = new
                {
                    from = model.From ?? DateTimeOffset.UtcNow.AddHours(-1).ToString("o"),
                    to = model.To ?? DateTimeOffset.UtcNow.ToString("o"),
                    query = BuildQuery(model)
                },
                page = new { limit = model.Limit > 20 ? 20 : model.Limit },

                sort = "timestamp"
            };
            using var http = CreateHttpClient();

            var res = await http.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();

            var result = new List<TraceLog>();
            dynamic doc = JsonConvert.DeserializeObject(json)!;
            foreach (var d in doc.data)
            {
                string ts = d.attributes?.timestamp != null ? (string)d.attributes.timestamp : "";
                string msg = d.attributes?.attributes?.message != null ? (string)d.attributes.attributes.message :
                             d.attributes?.message != null ? (string)d.attributes.message : "";
                result.Add(new TraceLog(
                        DateTimeOffset.Parse(ts),
                        msg,
                        d.attributes?.attributes?.level != null ? (string)d.attributes.attributes.level : "INFO",
                        SourceType.Datadog.ToString(),
                        d.id != null ? (string)d.id : "",
                        JObject.FromObject(d).ToString(Formatting.None)
                    ));
            }
            return result;
        }
        public HttpClient CreateHttpClient()
        {
            var http = new HttpClient();
            http.BaseAddress = new Uri(_datadogSettings.Site);
            http.DefaultRequestHeaders.Add("DD-API-KEY", _datadogSettings.ApiKey);
            http.DefaultRequestHeaders.Add("DD-APPLICATION-KEY", _datadogSettings.AppKey);
            return http;
        }
        private string? BuildQuery(LogQueryContext context)
        {
            var filters = new List<string>();

            // Service
            if (!string.IsNullOrWhiteSpace(context.ServiceName))
                filters.Add($"service:{context.ServiceName}");

            // Environment
            if (!string.IsNullOrWhiteSpace(context.Environment))
                filters.Add($"env:{context.Environment}");

            // Host
            if (!string.IsNullOrWhiteSpace(context.Host))
                filters.Add($"host:{context.Host}");

            // Application (Datadog may not have a direct 'application' field, but you can use a tag)
            //if (!string.IsNullOrWhiteSpace(context.Application))
            //    filters.Add($"application:{context.Application}");

            //// Region
            //if (!string.IsNullOrWhiteSpace(context.Region))
            //    filters.Add($"region:{context.Region}");

            // Level
            if (context.Levels.Any())
                filters.Add($"status: ({string.Join(" OR ", context.Levels)})");

            // Keyword (free text search)
            if (!string.IsNullOrWhiteSpace(context.Keyword))
                filters.Add(context.Keyword);

            // Tags
            if (context.Tags != null && context.Tags.Length > 0)
            {
                foreach (var tag in context.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                        filters.Add(tag);
                }
            }

            // Combine all filters with spaces (Datadog uses space as AND)
            return string.Join(" ", filters);
        }


    }
}
