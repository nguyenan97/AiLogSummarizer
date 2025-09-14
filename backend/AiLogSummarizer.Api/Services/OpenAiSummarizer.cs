using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiLogSummarizer.Api.Models;

namespace AiLogSummarizer.Api.Services;

public class OpenAiSummarizer : ILogSummarizer
{
    private readonly HttpClient _http = new();
    private readonly IConfiguration _config;

    public OpenAiSummarizer(IConfiguration config)
    {
        _config = config;
        var apiKey = _config["OpenAI:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<LogSummary> SummarizeAsync(SummarizeRequest request)
    {
        var prompt = $"Summarize the following logs and respond in JSON with properties RootCause, KeyErrors, FixSuggestions:\n{request.RawLog}";
        var payload = new
        {
            model = _config["OpenAI:Model"],
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var response = await _http.PostAsync(_config["OpenAI:Endpoint"],
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
        var json = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var content = doc.RootElement.GetProperty("choices")[0]
                .GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            var summary = JsonSerializer.Deserialize<LogSummary>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return summary ?? new LogSummary { RootCause = "Unable to parse AI JSON" };
        }
        catch
        {
            return new LogSummary { RootCause = "Unable to parse AI JSON" };
        }
    }
}
