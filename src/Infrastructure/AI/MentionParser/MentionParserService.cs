using System.Collections.Concurrent;
using Application.Interfaces;
using Azure;
using Azure.AI.OpenAI;
using Domain.MentionParsing.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Infrastructure.AI.MentionParser;

public sealed class MentionParserService : IMentionParserService
{
    private readonly IChatClient _chatClient;
    private readonly MentionParserOptions _options;
    private readonly ILogger<MentionParserService> _logger;
    private readonly ConcurrentDictionary<IntentType, string> _promptCache = new();

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
        var parameters = await InvokeAgentAsync(routerDecision.Intent, userText, cancellationToken).ConfigureAwait(false);

        return new MentionParsed
        {
            Intent = routerDecision.Intent,
            Category = routerDecision.Category,
            Suggestion = routerDecision.Suggestion,
            Parameters = parameters
        };
    }

    private async Task<RouterDecision> InvokeRouterAsync(string userText, CancellationToken cancellationToken)
    {
        var prompt = LoadPrompt(IntentType.Router);
        var messages = BuildMessages(prompt, userText, "router");
        var response = await GetStructuredAsync<RouterDecision>(messages, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private async Task<ICaseParameters> InvokeAgentAsync(IntentType intent, string userText, CancellationToken cancellationToken)
    {
        var prompt = LoadPrompt(intent);
        var messages = BuildMessages(prompt, userText, intent.ToString());

        ICaseParameters result = intent switch
        {
            IntentType.LatestErrors => await GetStructuredAsync<LatestErrorsParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.TimeRangeErrors => await GetStructuredAsync<TimeRangeErrorsParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.RootCauseByErrorCode => await GetStructuredAsync<RootCauseByErrorCodeParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.SearchByKeywordException => await GetStructuredAsync<SearchByKeywordExceptionParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.CorrelateByTraceId => await GetStructuredAsync<CorrelateByTraceIdParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.CrossServiceCorrelation => await GetStructuredAsync<CrossServiceCorrelationParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.RegressionAfterDeploy => await GetStructuredAsync<RegressionAfterDeployParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.LeakOrSlowBurnTrace => await GetStructuredAsync<LeakOrSlowBurnParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.PolicyRunbookFix => await GetStructuredAsync<PolicyRunbookFixParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.ManagerReport => await GetStructuredAsync<ManagerReportParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.FreeformLogQa => await GetStructuredAsync<FreeformLogQaParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.SecurityRelated => await GetStructuredAsync<SecurityRelatedParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.SlaSloMonitoring => await GetStructuredAsync<SlaSloMonitoringParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.TemporaryAlert => await GetStructuredAsync<TemporaryAlertParams>(messages, cancellationToken).ConfigureAwait(false),
            IntentType.GeneralAnalysis => await GetStructuredAsync<GeneralAnalysisParams>(messages, cancellationToken).ConfigureAwait(false),
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
        var resolvedSystemPrompt = systemPrompt
            .Replace("{{MODE}}", mode, StringComparison.OrdinalIgnoreCase)
            .Replace("{{TIMEZONE}}", _options.TimeZone, StringComparison.OrdinalIgnoreCase)
            .Replace("{{REFERENCE_UTC}}", now.ToString("O"), StringComparison.OrdinalIgnoreCase);

        return new List<Microsoft.Extensions.AI.ChatMessage>
        {
            new(ChatRole.System, resolvedSystemPrompt),
            new(ChatRole.User, userText)
        };
    }

    private string LoadPrompt(IntentType intent)
    {
        return _promptCache.GetOrAdd(intent, key => ReadPrompt(GetPromptFileName(key)));
    }

    private string GetPromptFileName(IntentType intent) => intent switch
    {
        IntentType.LatestErrors => "latest_errors.md",
        IntentType.TimeRangeErrors => "time_range_errors.md",
        IntentType.RootCauseByErrorCode => "root_cause_by_error_code.md",
        IntentType.SearchByKeywordException => "search_by_keyword_exception.md",
        IntentType.CorrelateByTraceId => "correlate_by_trace_id.md",
        IntentType.CrossServiceCorrelation => "cross_service_correlation.md",
        IntentType.RegressionAfterDeploy => "regression_after_deploy.md",
        IntentType.LeakOrSlowBurnTrace => "leak_or_slow_burn_trace.md",
        IntentType.PolicyRunbookFix => "policy_runbook_fix.md",
        IntentType.ManagerReport => "manager_report.md",
        IntentType.FreeformLogQa => "freeform_log_qa.md",
        IntentType.SecurityRelated => "security_related.md",
        IntentType.SlaSloMonitoring => "sla_slo_monitoring.md",
        IntentType.TemporaryAlert => "temporary_alert.md",
        IntentType.Router => "router.md",
        IntentType.GeneralAnalysis => "general_analysis.md",
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



