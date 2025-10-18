using System.Globalization;
using System.Text;
using Application.Interfaces;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;
using Humanizer;
using Infrastructure.Services.LogSources;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogReader.Services.Sources
{
    public class DatadogLogSource : ILogSourceService
    {
        private readonly DatadogSettings _datadogSettings;
        private readonly ILogger<DatadogLogSource> _logger;
        private readonly MentionParserOptions _mentionParserOptions;
        private readonly IMentionParserService _mentionParserService;
        public DatadogLogSource(IOptions<DatadogSettings> _datadogSettingsOption, ILogger<DatadogLogSource> logger,
            IOptions<MentionParserOptions> _MentionParserOptionsOptionValue,
            IMentionParserService mentionParserService
            ) // Fix the type name to IOptions<>
        {
            _datadogSettings = _datadogSettingsOption.Value;
            _logger = logger;
            _mentionParserOptions= _MentionParserOptionsOptionValue.Value;
            _mentionParserService = mentionParserService;
        }
        // TODO: Add support for SQL-like trace queries (e.g., SELECT * FROM logs WHERE error_code = '500')
        // Next step: Implement query builder for complex trace correlations
        // Improvement: Add caching layer for frequently accessed logs
        public async Task<IEnumerable<TraceLog>> GetLogsAsync(LogQueryContext model)
        {
            var url = $"/api/v2/logs/events/search";

            object? request;
            if (_datadogSettings.BuildQueryWithAI)
            {
                var builder = new DataDogQueryBuilder(model,_mentionParserOptions,_mentionParserService);
                request =await builder.BuidQueryWithAI().ConfigureAwait(false);
            }
            else
            {
                var builder = new DataDogQueryBuilder(model);
                request = builder.BuildRequest();
            }
            _logger.LogInformation("Datadog Query {query}", request);
            using var http = CreateHttpClient();

            var res = await http.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var json = await res.Content.ReadAsStringAsync();
            var result = new List<TraceLog>();
            if (res.IsSuccessStatusCode)
            {

                dynamic doc = JsonConvert.DeserializeObject(json)!;
                foreach (var d in doc.data)
                {
                    string ts = d.attributes?.timestamp != null ? (string)d.attributes.timestamp : "";
                    string msg = d.attributes?.attributes?.message != null ? (string)d.attributes.attributes.message :
                                 d.attributes?.message != null ? (string)d.attributes.message : "";
                    result.Add(new TraceLog(
                            DateTimeOffset.Parse(ts, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                            msg,
                            d.attributes?.attributes?.level != null ? (string)d.attributes.attributes.level : "INFO",
                            SourceType.Datadog.ToString(),
                            d.id != null ? (string)d.id : "",
                            JObject.FromObject(d).ToString(Formatting.None)
                        ));
                }
            }
            else
            {
                _logger.LogError(json);
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



    }
}
