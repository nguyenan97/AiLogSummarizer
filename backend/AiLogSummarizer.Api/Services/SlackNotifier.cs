using System.Text;
using System.Text.Json;
using AiLogSummarizer.Api.Models;

namespace AiLogSummarizer.Api.Services;

public class SlackNotifier
{
    private readonly HttpClient _http = new();
    private readonly IConfiguration _config;

    public SlackNotifier(IConfiguration config) => _config = config;

    public async Task NotifyAsync(LogSummary summary)
    {
        var url = _config["Slack:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(url)) return;

        var payload = JsonSerializer.Serialize(new
        {
            text = $"RootCause: {summary.RootCause}\nKeyErrors: {string.Join(", ", summary.KeyErrors)}"
        });
        await _http.PostAsync(url,
            new StringContent(payload, Encoding.UTF8, "application/json"));
    }
}
