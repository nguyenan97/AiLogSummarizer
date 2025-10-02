using System.Text;
using Application.Interfaces;
using Domain.Models;
using Domain.Shared;
using Humanizer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        public async Task<IEnumerable<TraceLog>> GetLogsAsync(GetLogModel model)
        {
            var url = $"{_datadogSettings.Site}/api/v2/logs/events/search";
            var payload = new
            {
                filter = new
                {
                    from = model.StartTime.ToString("o"),
                    to = model.EndTime.ToString("o"),
                    query = string.IsNullOrWhiteSpace(model.Query) ? _datadogSettings.DefaultQuery : model.Query
                },
                page = new { limit = 100 },
                sort = "timestamp"
            };
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("DD-API-KEY", _datadogSettings.ApiKey);
            http.DefaultRequestHeaders.Add("DD-APPLICATION-KEY", _datadogSettings.AppKey);

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
                result.Add(new TraceLog(DateTimeOffset.Parse(ts), msg, d.attributes?.attributes?.status != null ? (string)d.attributes.attributes.status : "INFO", SourceType.Datadog.ToString()));
            }
            return result;
        }


    }
}
