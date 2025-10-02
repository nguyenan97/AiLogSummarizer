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
        public async Task<IEnumerable<TraceLog>> GetLogsAsync(GetLogModel model)
        {
            var files = Directory.Exists(_settings.Path) ? Directory.GetFiles(_settings.Path, _settings.FilePattern, SearchOption.AllDirectories) : Array.Empty<string>();
            var sb = new StringBuilder();
            var result = new List<TraceLog>();
            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                if (fi.LastWriteTimeUtc < model.StartTime.UtcDateTime || fi.LastWriteTimeUtc > model.EndTime.UtcDateTime) continue;
                foreach (var line in File.ReadLines(f))
                {
                    if (!string.IsNullOrWhiteSpace(model.Query)
                    && !line.Contains("error", StringComparison.OrdinalIgnoreCase)
                        && !line.Contains(model.Query, StringComparison.OrdinalIgnoreCase)) continue;
                    result.Add(new TraceLog(fi.LastWriteTimeUtc, line, "INFO", SourceType.Folder.ToString()));
                }
            }
            return await Task.FromResult(result);
        }
    }
}
