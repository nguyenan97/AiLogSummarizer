using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Application.Interfaces;
using Azure;
using Azure.AI.OpenAI;
using Domain.MentionParsing.Models;
using Domain.Models;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Infrastructure.Services.LogSources;
public class DataDogQueryBuilder
{
    private readonly LogQueryContext _model;
    private readonly MentionParserOptions _options=new MentionParserOptions();
    private readonly IMentionParserService _mentionParserService = null!;

    public DataDogQueryBuilder(LogQueryContext model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
       
    }
    public DataDogQueryBuilder(LogQueryContext model, MentionParserOptions options, IMentionParserService mentionParserService)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _options = options ?? throw new ArgumentNullException( nameof(options));
        _mentionParserService = mentionParserService;

    }
    private static IChatClient CreateChatClient(MentionParserOptions options)
    {
        var endpoint = new Uri(options.Endpoint, UriKind.Absolute);

        AzureOpenAIClient azure = new AzureOpenAIClient(endpoint, new AzureKeyCredential(options.ApiKey!));

        ChatClient azureChat = azure.GetChatClient(options.DeploymentName);

        IChatClient baseClient = azureChat.AsIChatClient();

        return new ChatClientBuilder(baseClient)
            .UseFunctionInvocation()
            .Build();
    }
    public  async Task<object?> BuidQueryWithAI()
    {
        // Validate MentionParserOptions
        if (_options == null)
            throw new InvalidOperationException("MentionParserOptions must be provided.");

        // Create ChatClient using the provided options
        var chatClient = CreateChatClient(_options);

        // Prepare the prompt for AI
        var prompt = new StringBuilder();
        var instruction = _mentionParserService.ReadPrompt("datadog_query_convert.md");

       

        // Create a chat message
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.Assistant,instruction),
            new ChatMessage(ChatRole.User,JsonConvert.SerializeObject(_model))
        };
        try
        {
            // Send the request to the AI model
            var response = await chatClient.GetResponseAsync<object>(messages);
            return JObject.Parse(response.Text);
        }
        catch(Exception ex)
        {
            throw ex;
        }

        
      
    }
    /// <summary>
    /// Builds the complete DataDog query string based on the model parameters
    /// </summary>
    public string BuildQuery()
    {
        var queryParts = new List<string>();

        // Add service filter
        if (_model.ServiceNames != null && _model.ServiceNames.Any())
        {
            var serviceQuery = string.Join(" OR ", _model.ServiceNames.Select(s => $"service:{EscapeValue(s)}"));
            if (_model.ServiceNames.Count > 1)
            {
                queryParts.Add($"({serviceQuery})");
            }
            else
            {
                queryParts.Add(serviceQuery);
            }
        }

        // Add environment filter
        if (!string.IsNullOrWhiteSpace(_model.Environment))
        {
            queryParts.Add($"env:{EscapeValue(_model.Environment)}");
        }

        // Add host filter
        if (!string.IsNullOrWhiteSpace(_model.Host))
        {
            queryParts.Add($"host:{EscapeValue(_model.Host)}");
        }

        // Add log levels
        if (_model.Levels != null && _model.Levels.Any())
        {
            var levelQuery = string.Join(" OR ", _model.Levels.Select(l => $"status:{EscapeValue(l.ToLower())}"));
            if (_model.Levels.Count > 1)
            {
                queryParts.Add($"({levelQuery})");
            }
            else
            {
                queryParts.Add(levelQuery);
            }
        }

        // Add keywords (search in message content)
        if (_model.Keywords != null && _model.Keywords.Any())
        {
            var keywordParts = new List<string>();

            foreach (var keyword in _model.Keywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    // Add wildcard search in @exception field
                    keywordParts.Add($"@Exception:*{EscapeWildcardValue(keyword)}*");

                    // Also add exact phrase search in message
                    keywordParts.Add($"{EscapeWildcardValue(keyword)}");
                }
            }

            if (keywordParts.Any())
            {
                var keywordQuery = string.Join(" OR ", keywordParts);
                queryParts.Add($"({keywordQuery})");
            }

        }

        // Add custom tags
        if (_model.Tags != null && _model.Tags.Any())
        {
            var tagQuery = string.Join(" OR ", _model.Tags.Select(tag => $"{EscapeValue(tag.Key)}:{EscapeValue(tag.Value)}"));
            if (_model.Tags.Count > 1)
            {

                queryParts.Add($"({tagQuery})");
            }
            else
            {
                queryParts.Add(tagQuery);
            }


        }

        return string.Join(" ", queryParts);
    }

    /// <summary>
    /// Builds the complete request parameters for DataDog Logs API
    /// </summary>
    public object BuildRequest()
    {
        return new
        {
            filter = new
            {
                from = ParseTimestamp(_model.From, DateTime.UtcNow.AddDays(-1)),
                to = ParseTimestamp(_model.To, DateTime.UtcNow),
                query = BuildQuery(),

            },
            page = new
            {
                limit = _model.Limit == 0 ? 5 : _model.Limit
            },
            sort = "-timestamp"
        };
    }



    /// <summary>
    /// Escapes special characters in query values
    /// </summary>
    private string EscapeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        // Escape special characters for DataDog query syntax
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace(":", "\\:");
    }
    private string EscapeWildcardValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        // Escape special characters but preserve wildcards
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace(":", "\\:")
            .Replace(" ", "\\ "); // Escape spaces in wildcard queries
    }
    /// <summary>
    /// Parses timestamp to ISO 8601 format or relative time
    /// Supports: ISO dates, relative times (e.g., "now-1h", "now-7d"), epoch milliseconds
    /// </summary>
    private string ParseTimestamp(string? timestamp, DateTime defaultTimeIfNull)
    {
        if (string.IsNullOrWhiteSpace(timestamp))
            return defaultTimeIfNull.ToString("o"); // Default: 1 date ago

        // If already in relative format (e.g., "now-1h")
        if (timestamp.StartsWith("now", StringComparison.OrdinalIgnoreCase))
            return timestamp;

        // Try parse as DateTime
        if (DateTime.TryParse(timestamp, out var dt))
        {
            return dt.ToUniversalTime().ToString("o");
        }

        // Try parse as Unix timestamp (milliseconds)
        if (long.TryParse(timestamp, out var unixMs))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime.ToString("o");
        }

        // Default fallback
        return timestamp;
    }

}

/// <summary>
/// DataDog Logs API request model
/// </summary>
public class DataDogLogsRequest
{
    public string Query { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public int Limit { get; set; } = 5;
    public string Sort { get; set; } = "desc";
}

/// <summary>
/// Example usage and helper extensions
/// </summary>
public static class DataDogQueryBuilderExtensions
{
    public static DataDogQueryBuilder ToDataDogQuery(this LogQueryContext model)
    {
        return new DataDogQueryBuilder(model);
    }
}

// Example usage:
/*
var model = new LogQueryModel
{
    Source = SourceType.Datadog,
    From = "now-24h",
    To = "now",
    Environment = "prod",
    ServiceNames = new List<string> { "PaymentService", "APIInternal" },
    Host = "web-server-01",
    Levels = new List<string> { "error", "warning" },
    Keywords = new List<string> { "TimeoutException", "JWT" },
    Tags = new Dictionary<string, string>
    {
        { "team", "backend" },
        { "version", "1.2.3" }
    },
    Limit = 100
};

var builder = new DataDogQueryBuilder(model);

// Get query string
string query = builder.BuildQuery();
// Output: (service:PaymentService OR service:APIInternal) env:prod host:web-server-01 (status:error OR status:warning) "TimeoutException" "JWT" team:backend version:1.2.3

// Get full request
var request = builder.BuildRequest();

// Get query parameters for HTTP request
var parameters = builder.BuildQueryParameters();

// Get human-readable description
string description = builder.GetQueryDescription();
*/