using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Models;
using Domain.Shared;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LogReader.Services.Sources
{
    public class FolderLogSource : ILogSourceService
    {
        private readonly LogFolderSettings _settings;
        public FolderLogSource(IOptions<LogFolderSettings> _settingsOption)
        {
            _settings = _settingsOption.Value;
        }
        public async Task<IEnumerable<TraceLog>> GetLogsAsync(LogQueryContext model)
        {
            var files = Directory.Exists(_settings.Path) ? Directory.GetFiles(_settings.Path, _settings.FilePattern, SearchOption.AllDirectories) : Array.Empty<string>();
            var sb = new StringBuilder();
            var result = new List<TraceLog>();
            var fromDate = model.From!=null? DateTimeOffset.Parse(model.From).ToUniversalTime():new DateTimeOffset(DateTime.UtcNow.AddHours(-1));
            var toDate = model.To != null ? DateTimeOffset.Parse(model.To).ToUniversalTime() : new DateTimeOffset(DateTime.UtcNow);
            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                if (fi.LastWriteTimeUtc < fromDate || fi.LastWriteTimeUtc > toDate) continue;
                foreach (var line in File.ReadLines(f))
                {
                    var query =BuildQuery(model);
                    if (!string.IsNullOrWhiteSpace(query)
                    && !line.Contains("error", StringComparison.OrdinalIgnoreCase)
                        && !line.Contains(query, StringComparison.OrdinalIgnoreCase)) continue;

                    result.Add(new TraceLog(
                        fi.LastWriteTimeUtc,
                        line,
                        "INFO",
                        SourceType.Folder.ToString(),
                        GenerateId(line),
                        line
                    ));
                   
                }
            }
            return await Task.FromResult(result);
        }

        public Task<TraceLog?> GetLogDetailAsync(string traceId)
        {
            throw new NotImplementedException();
        }
        private string BuildQuery(LogQueryContext model)
        {
            return string.Empty;
        }

        private string GenerateId(string input)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }
    }
}
