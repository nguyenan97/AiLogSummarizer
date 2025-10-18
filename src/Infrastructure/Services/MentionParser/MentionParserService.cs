using System;
using System.Collections.Concurrent;
using System.Linq;
using Application.Interfaces;
using Azure;
using Azure.AI.OpenAI;
using Domain.MentionParsing.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Infrastructure.Services.MentionParser;

public sealed class MentionParserService : IMentionParserService
{
    private readonly IChatClient _chatClient;
    private readonly MentionParserOptions _options;
    private readonly ILogger<MentionParserService> _logger;
    private readonly ConcurrentDictionary<IntentDetail, string> _promptCache = new();
    private readonly IHistoryLayerService _historyLayerService;
    private readonly ConcurrentDictionary<string, List<Microsoft.Extensions.AI.ChatMessage>> _memoryMessagesCache = new();

    public MentionParserService(IOptions<MentionParserOptions> options, ILogger<MentionParserService> logger, IHistoryLayerService historyLayerService)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _historyLayerService = historyLayerService ?? throw new ArgumentNullException(nameof(historyLayerService));

        _chatClient = CreateChatClient(_options);
    }

    public async Task<MentionParsed> ParseAsync(string userText, string? conversationKey = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
        {
            throw new ArgumentException("User text cannot be null or empty.", nameof(userText));
        }

        var routerDecision = await InvokeRouterAsync(userText, conversationKey, cancellationToken).ConfigureAwait(false);
        var parameters = await InvokeAgentAsync(routerDecision.IntentDetail, userText, routerDecision.Language, conversationKey, cancellationToken).ConfigureAwait(false);

        return new MentionParsed
        {
            Intent = routerDecision.Intent,
            IntentDetail = routerDecision.IntentDetail,
            Parameters = parameters,
            Language = routerDecision.Language
        };
    }

    private async Task<RouterDecision> InvokeRouterAsync(string userText, string? conversationKey, CancellationToken cancellationToken)
    {
        var prompt = LoadPrompt(IntentDetail.Router);
        var messages = await BuildMessagesAsync(prompt, userText, "router", InputLanguage.Unknown, conversationKey, cancellationToken).ConfigureAwait(false);
        var routerPromptFile = GetPromptFileName(IntentDetail.Router);
        _logger.LogInformation("AI PromptName Stage={Stage} Intent={Intent} PromptFile={PromptFile}", "router", IntentDetail.Router, routerPromptFile);
        var response = await GetStructuredAsync<RouterDecision>(messages, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private async Task<ICaseParameters> InvokeAgentAsync(IntentDetail intent, string userText, InputLanguage language, string? conversationKey, CancellationToken cancellationToken)
    {
        var prompt = LoadPrompt(intent);
        var messages = await BuildMessagesAsync(prompt, userText, intent.ToString(), language, conversationKey, cancellationToken).ConfigureAwait(false);
        var agentPromptFile = GetPromptFileName(intent);
        _logger.LogInformation("AI PromptName Stage={Stage} Intent={Intent} PromptFile={PromptFile}", "agent", intent, agentPromptFile);

        // For all recognized intents, unify on LogQueryCaseParams to simplify downstream processing.
        // Unknown intent still returns UnknownIntentParams for UX messaging.
        if (intent == IntentDetail.Unknown || intent == IntentDetail.Router)
        {
            var unknown = await GetStructuredAsync<UnknownIntentParams>(messages, cancellationToken).ConfigureAwait(false);
            return unknown ?? new UnknownIntentParams();
        }

        var unified = await GetStructuredAsync<LogQueryCaseParams>(messages, cancellationToken).ConfigureAwait(false);
        return unified ?? new LogQueryCaseParams();
    }


    private async Task<T> GetStructuredAsync<T>(
    IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
    CancellationToken cancellationToken)
    {
        var response = await _chatClient.GetResponseAsync<T>(
            messages,
            cancellationToken: cancellationToken);

        return response.Result;
    }

    private async Task<List<Microsoft.Extensions.AI.ChatMessage>> BuildMessagesAsync(string systemPrompt, string userText, string mode, InputLanguage language = InputLanguage.Unknown, string? conversationKey = null, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var knownServices = string.Join(", ", _options.KnownServices ?? Array.Empty<string>());
        var knownEnvironments = string.Join(", ", _options.KnownEnvironments ?? Array.Empty<string>());
        var intentTypes = string.Join(", ", GetEnumMemberValues<IntentType>());
        var intentDetails = string.Join(", ", GetEnumMemberValues<IntentDetail>());
        var inputLanguages = string.Join(", ", GetEnumMemberValues<InputLanguage>());
        var languageCode = language switch
        {
            InputLanguage.Vietnamese => "vi",
            InputLanguage.English => "en",
            _ => "en"
        };
        string resolvedSystemPrompt = systemPrompt
            .Replace("{{MODE}}", mode, StringComparison.OrdinalIgnoreCase)
            .Replace("{{TIMEZONE}}", _options.TimeZone, StringComparison.OrdinalIgnoreCase)
            .Replace("{{REFERENCE_UTC}}", now.ToString("O"), StringComparison.OrdinalIgnoreCase)
            .Replace("{{KNOWN_SERVICES}}", knownServices, StringComparison.OrdinalIgnoreCase)
            .Replace("{{KNOWN_ENVIRONMENTS}}", knownEnvironments, StringComparison.OrdinalIgnoreCase)
            .Replace("{{INTENT_TYPES}}", intentTypes, StringComparison.OrdinalIgnoreCase)
            .Replace("{{INTENT_DETAILS}}", intentDetails, StringComparison.OrdinalIgnoreCase)
            .Replace("{{INPUT_LANGUAGES}}", inputLanguages, StringComparison.OrdinalIgnoreCase)
            .Replace("{{LANGUAGE}}", languageCode, StringComparison.OrdinalIgnoreCase);

        var messages = new List<Microsoft.Extensions.AI.ChatMessage>
        {
            new(ChatRole.System, resolvedSystemPrompt)
        };

        // Add conversation memories as separate messages (cached per conversationKey for this scoped service)
        if (!string.IsNullOrWhiteSpace(conversationKey))
        {
            var memoryMessages = await GetMemoryMessagesAsync(conversationKey!, cancellationToken).ConfigureAwait(false);
            if (memoryMessages.Count > 0)
            {
                messages.AddRange(memoryMessages);
            }
        }

        messages.Add(new(ChatRole.User, userText));
        return messages;
    }

    private async Task<List<Microsoft.Extensions.AI.ChatMessage>> GetMemoryMessagesAsync(string conversationKey, CancellationToken cancellationToken)
    {
        if (_memoryMessagesCache.TryGetValue(conversationKey, out var cached))
        {
            return cached;
        }

        var memories = await _historyLayerService.GetUserMemoriesAsync(conversationKey);
        var top = memories?
            .Where(m => !string.IsNullOrWhiteSpace(m.Memory) || !string.IsNullOrWhiteSpace(m.Text))
            .Take(20)
            .ToList();

        var result = new List<Microsoft.Extensions.AI.ChatMessage>();
        if (top is { Count: > 0 })
        {
            var memoryBlock = string.Join("\n", top.Select(m => $"- {(m.Memory ?? m.Text)!.Trim()}"));
            result.Add(new(ChatRole.System, "[Keywords: ]\n" + memoryBlock));
        }

        _memoryMessagesCache[conversationKey] = result;
        return result;
    }

    private static IEnumerable<string> GetEnumMemberValues<TEnum>() where TEnum : struct, Enum
    {
        foreach (var value in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            if (memberInfo is not null)
            {
                var attr = memberInfo
                    .GetCustomAttributes(typeof(System.Runtime.Serialization.EnumMemberAttribute), false)
                    .Cast<System.Runtime.Serialization.EnumMemberAttribute>()
                    .FirstOrDefault();
                if (attr?.Value is string s && !string.IsNullOrWhiteSpace(s))
                {
                    yield return s;
                    continue;
                }
            }
            yield return value.ToString();
        }
    }

    private string LoadPrompt(IntentDetail intent)
    {
        return _promptCache.GetOrAdd(intent, key => ReadPrompt(GetPromptFileName(key)));
    }

    private string GetPromptFileName(IntentDetail intent) => intent switch
    {
        IntentDetail.LatestErrors => "latest_errors.md",
        IntentDetail.TimeRangeErrors => "time_range_errors.md",
        IntentDetail.RootCauseByErrorCode => "root_cause_by_error_code.md",
        IntentDetail.SearchByKeywordException => "search_by_keyword_exception.md",
        IntentDetail.CorrelateByTraceId => "correlate_by_trace_id.md",
        IntentDetail.CrossServiceCorrelation => "cross_service_correlation.md",
        IntentDetail.RegressionAfterDeploy => "regression_after_deploy.md",
        IntentDetail.LeakOrSlowBurnTrace => "leak_or_slow_burn_trace.md",
        IntentDetail.PolicyRunbookFix => "policy_runbook_fix.md",
        IntentDetail.ManagerReport => "manager_report.md",
        IntentDetail.FreeformLogQa => "freeform_log_qa.md",
        IntentDetail.SecurityRelated => "security_related.md",
        IntentDetail.SlaSloMonitoring => "sla_slo_monitoring.md",
        IntentDetail.TemporaryAlert => "temporary_alert.md",
        IntentDetail.Router => "Router.md",
        IntentDetail.GeneralAnalysis => "general_analysis.md",
        _ => "unknown.md"
    };

    public string ReadPrompt(string fileName)
    {
        var promptDirectory = ResolvePromptDirectory();
        var path = Path.Combine(promptDirectory, fileName);
        if (!File.Exists(path))
        {
            _logger.LogWarning("Prompt file '{PromptFile}' was not found at '{PromptDirectory}'.", fileName, promptDirectory);
            return string.Empty;
        }
        return File.ReadAllText(path);
    }

    private static string ResolvePromptDirectory()
    {
        var caPath = Path.Combine(AppContext.BaseDirectory, "Application", "MentionParsing", "Prompts");
        if (Directory.Exists(caPath)) return caPath;

        var legacyPath = Path.Combine(AppContext.BaseDirectory, "MentionParsing", "Prompts");
        return Directory.Exists(legacyPath) ? legacyPath : AppContext.BaseDirectory;
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
}



