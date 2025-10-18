using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.MentionParsing.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntentDetail
{
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    [EnumMember(Value = "Router")]
    Router,

    [EnumMember(Value = "latest_errors")]
    LatestErrors,

    [EnumMember(Value = "time_range_errors")]
    TimeRangeErrors,

    [EnumMember(Value = "root_cause_by_error_code")]
    RootCauseByErrorCode,

    [EnumMember(Value = "search_by_keyword_exception")]
    SearchByKeywordException,

    [EnumMember(Value = "correlate_by_trace_id")]
    CorrelateByTraceId,

    [EnumMember(Value = "cross_service_correlation")]
    CrossServiceCorrelation,

    [EnumMember(Value = "regression_after_deploy")]
    RegressionAfterDeploy,

    [EnumMember(Value = "leak_or_slow_burn_trace")]
    LeakOrSlowBurnTrace,

    [EnumMember(Value = "policy_runbook_fix")]
    PolicyRunbookFix,

    [EnumMember(Value = "manager_report")]
    ManagerReport,

    [EnumMember(Value = "freeform_log_qa")]
    FreeformLogQa,

    [EnumMember(Value = "security_related")]
    SecurityRelated,

    [EnumMember(Value = "sla_slo_monitoring")]
    SlaSloMonitoring,

    [EnumMember(Value = "temporary_alert")]
    TemporaryAlert,

    [EnumMember(Value = "general_analysis")]
    GeneralAnalysis
}