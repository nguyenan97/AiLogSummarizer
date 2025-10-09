using System.Collections.Concurrent;
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

    public MentionParserService(IOptions<MentionParserOptions> options, ILogger<MentionParserService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _chatClient = CreateChatClient(_options);
    }

    public async Task<MentionParsed> ParseAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
        {
            throw new ArgumentException("User text cannot be null or empty.", nameof(userText));
        }

        var routerDecision = await InvokeRouterAsync(userText, cancellationToken).ConfigureAwait(false);
        var parameters = await InvokeAgentAsync(routerDecision.IntentDetail, userText, cancellationToken).ConfigureAwait(false);

        return new MentionParsed
        {
            Intent = routerDecision.Intent,
            IntentDetail = routerDecision.IntentDetail,
            Parameters = parameters
        };
    }

    private async Task<RouterDecision> InvokeRouterAsync(string userText, CancellationToken cancellationToken)
    {
        var prompt = LoadPrompt(IntentDetail.Router);
        var messages = BuildMessages(prompt, userText, "router");
        var response = await GetStructuredAsync<RouterDecision>(messages, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private async Task<ICaseParameters> InvokeAgentAsync(IntentDetail intent, string userText, CancellationToken cancellationToken)
    {
        var prompt = LoadPrompt(intent);
        var messages = BuildMessages(prompt, userText, intent.ToString());

        ICaseParameters result = intent switch
        {
            IntentDetail.LatestErrors => await GetStructuredAsync<LatestErrorsParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.TimeRangeErrors => await GetStructuredAsync<TimeRangeErrorsParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.RootCauseByErrorCode => await GetStructuredAsync<RootCauseByErrorCodeParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.SearchByKeywordException => await GetStructuredAsync<SearchByKeywordExceptionParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.CorrelateByTraceId => await GetStructuredAsync<CorrelateByTraceIdParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.CrossServiceCorrelation => await GetStructuredAsync<CrossServiceCorrelationParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.RegressionAfterDeploy => await GetStructuredAsync<RegressionAfterDeployParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.LeakOrSlowBurnTrace => await GetStructuredAsync<LeakOrSlowBurnParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.PolicyRunbookFix => await GetStructuredAsync<PolicyRunbookFixParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.ManagerReport => await GetStructuredAsync<ManagerReportParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.FreeformLogQa => await GetStructuredAsync<FreeformLogQaParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.SecurityRelated => await GetStructuredAsync<SecurityRelatedParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.SlaSloMonitoring => await GetStructuredAsync<SlaSloMonitoringParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.TemporaryAlert => await GetStructuredAsync<TemporaryAlertParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentDetail.GeneralAnalysis => await GetStructuredAsync<GeneralAnalysisParams>(messages, cancellationToken).ConfigureAwait(false),
            _ => await GetStructuredAsync<UnknownIntentParams>(messages, cancellationToken).ConfigureAwait(false)
        };

        return result ?? new UnknownIntentParams();
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

    private List<Microsoft.Extensions.AI.ChatMessage> BuildMessages(string systemPrompt, string userText, string mode)
    {
        var now = DateTimeOffset.UtcNow;
        var knownServices = string.Join(", ", _options.KnownServices ?? Array.Empty<string>());
        var knownEnvironments = string.Join(", ", _options.KnownEnvironments ?? Array.Empty<string>());
        var intentTypes = string.Join(", ", GetEnumMemberValues<IntentType>());
        var intentDetails = string.Join(", ", GetEnumMemberValues<IntentDetail>());

        var resolvedSystemPrompt = systemPrompt
            .Replace("{{MODE}}", mode, StringComparison.OrdinalIgnoreCase)
            .Replace("{{TIMEZONE}}", _options.TimeZone, StringComparison.OrdinalIgnoreCase)
            .Replace("{{REFERENCE_UTC}}", now.ToString("O"), StringComparison.OrdinalIgnoreCase)
            .Replace("{{KNOWN_SERVICES}}", knownServices, StringComparison.OrdinalIgnoreCase)
            .Replace("{{KNOWN_ENVIRONMENTS}}", knownEnvironments, StringComparison.OrdinalIgnoreCase)
            .Replace("{{INTENT_TYPES}}", intentTypes, StringComparison.OrdinalIgnoreCase)
            .Replace("{{INTENT_DETAILS}}", intentDetails, StringComparison.OrdinalIgnoreCase);

        return new List<Microsoft.Extensions.AI.ChatMessage>
        {
            new(ChatRole.System, resolvedSystemPrompt),
            new(ChatRole.User, userText)
        };
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

    private string ReadPrompt(string fileName)
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



