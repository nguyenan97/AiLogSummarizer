using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Azure.AI.OpenAI;
using Domain.Shared;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Infrastructure.Services;

public class MentionParserService : IMentionParserService
{
    private const string MentionParserSystemPrompt = """
You are an intelligent natural language parser for the Slack bot OopsAI.

Task:
Receive a Vietnamese natural language command from the user and return a normalized JSON describing:
- intent
- source
- start
- end
- errorCode (if any)
- suggestion (if any error occurs)

Mandatory Rules:

- Timezone: Asia/Ho_Chi_Minh (+07:00)
- Always return time in ISO-8601 format with +07:00 offset (e.g., 2025-01-20T07:00:00+07:00)
- Valid intent values: Analyze, Summarize, Report
- Valid source values: Datadog, Folder
- If the source is not specified -> default to "Datadog".
- If no time or range is provided -> suggest using today (full day from 00:00:00 to 23:59:59.999 in Asia/Ho_Chi_Minh timezone).
- Support:
  - "X hours recently" -> [now - X hours, now]
  - "yesterday" -> full day of yesterday if no time is provided; if "from ... to ..." is given, use that range
  - "morning" = AM, "afternoon/evening" = PM
  - Accept various date/time formats (e.g., d/M/yyyy, dd/MM/yyyy, H, H:mm, HH:mm, etc.)
- If the time range is invalid (start >= end) -> set success=false, errorCode="InvalidTimeRange", and provide a suggestion with one or two example commands with the correct syntax.
- If any other error occurs while parsing time:
  - Prefer defaulting to "today" if a reasonable assumption can be made.
  - If not possible, set success=false, errorCode="MissingTime", and provide a suggestion.
- If successful -> success=true, errorCode=null, suggestion=null.

Valid Examples:
- @OopsAI Analyze from 7h 20/1/2025 to 9h 21/1/2025 (datadog)
- @OopsAI Summarize last 2 hours
- @OopsAI Report today
- @OopsAI Report yesterday from 8am to 5pm (folder)

Sample JSON output (valid):
{
  "success": true,
  "intent": "Analyze",
  "source": "Datadog",
  "range": {
    "start": "2025-01-20T00:00:00+07:00",
    "end": "2025-01-20T23:59:59.999+07:00"
  },
  "errorCode": null,
  "suggestion": null
}

Sample JSON output (invalid range):
{
  "success": false,
  "intent": "Summarize",
  "source": "Datadog",
  "range": null,
  "errorCode": "InvalidTimeRange",
  "suggestion": "Summarize from 8h 20/1/2025 to 10h 20/1/2025 (datadog)"
}

Important Requirements:
- Always return a single valid JSON object only - no explanations, no markdown, no additional text.
- All parsing logic (intent, source, time, validation, suggestions) must be handled by the AI.
""";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ChatClient _chatClient;

    public MentionParserService(AzureOpenAIClient client, IOptions<AzureOpenAIOptions> options)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var settings = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
        _chatClient = client.GetChatClient(settings.DeploymentName);
    }

    public async Task<MentionParseResult> ParseMentionAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Mention text cannot be null or empty.", nameof(text));
        }

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(MentionParserSystemPrompt),
            new UserChatMessage(text)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0
        };

        ChatCompletion completion = await _chatClient
            .CompleteChatAsync(messages, options, cancellationToken)
            .ConfigureAwait(false);

        var content = completion.Content.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Mention parser returned an empty response.");
        }

        var result = JsonSerializer.Deserialize<MentionParseResult>(content, SerializerOptions);

        if (result is null)
        {
            throw new InvalidOperationException("Unable to deserialize mention parser response.");
        }

        return result;
    }
}

