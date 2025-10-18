using System.Collections.Generic;
using Domain.Shared;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Common contextual fields to scope Datadog log/APM/metrics queries.
/// Populate as much as possible from the user's mention; all fields are optional.
/// </summary>
public sealed record CaseContext
{
    ///// <summary>
    ///// Primary service name (normalized) e.g., "orders-api".
    ///// Prefer this when a single service is targeted.
    ///// </summary>
    //public string? Service { get; init; }

    ///// <summary>
    ///// Multiple services if the request spans several components.
    ///// Example: ["orders-api","payment-api"].
    ///// </summary>
    //public IReadOnlyList<string>? Services { get; init; }

    ///// <summary>
    ///// Deployment environment (normalized), e.g., "prod", "staging".
    ///// </summary>
    //public string? Environment { get; init; }

    ///// <summary>
    ///// Multiple environments if explicitly requested.
    ///// </summary>
    //public IReadOnlyList<string>? Environments { get; init; }

    ///// <summary>
    ///// IANA time zone for time parsing and display, e.g., "UTC".
    ///// </summary>
    //public string TimeZone { get; init; } = "UTC";

    ///// <summary>
    ///// Absolute start time in ISO8601 (UTC) when specified by the user.
    ///// </summary>
    //public string? FromIso { get; init; }

    ///// <summary>
    ///// Absolute end time in ISO8601 (UTC) when specified by the user.
    ///// </summary>
    //public string? ToIso { get; init; }

    ///// <summary>
    ///// Relative lookback window as ISO8601 duration, e.g., "PT2H", "P1D".
    ///// Use when the user provides a relative time instead of absolute range.
    ///// </summary>
    //public string? Lookback { get; init; }

    ///// <summary>
    ///// Arbitrary tag filters for Datadog queries, e.g., {"orderId":"9981"}.
    ///// Map known correlation keys to values for easier cross-service joins.
    ///// </summary>
    //public IReadOnlyDictionary<string, string>? Tags { get; init; }

    ///// <summary>
    ///// Primary trace identifier if present (APM/OTel trace id, hex or UUID).
    ///// </summary>
    //public string? TraceId { get; init; }

    ///// <summary>
    ///// Optional span identifier tied to the TraceId when provided.
    ///// </summary>
    //public string? SpanId { get; init; }

    ///// <summary>
    ///// Deployment marker reference (tag/commit/PR) used for regression checks.
    ///// </summary>
    //public string? DeployMarker { get; init; }

    ///// <summary>
    ///// Error or business code token, e.g., "ERR-1001", "504".
    ///// </summary>
    //public string? ErrorCode { get; init; }

    ///// <summary>
    ///// Severity hint (e.g., "critical", "error", "warning") if inferred.
    ///// </summary>
    //public string? Severity { get; init; }

    ///// <summary>
    ///// Optional Datadog query string or monitor query (raw DSL) when parsed.
    ///// </summary>
    //public string? DatadogQuery { get; init; }

    ///// <summary>
    ///// Optional limit for result counts (e.g., TopN) used by some queries.
    ///// </summary>
    //public int? Limit { get; init; }


    public SourceType Source { get; set; } = SourceType.Datadog; // "datadog", "application-insights", "elk", v.v.
    public string? From { get; set; }
    public string? To { get; set; }

    // ===== 2. Environment/Scope =====
    public string? Environment { get; set; }                 // "prod", "staging", "dev"
    public List<string> ServiceNames { get; set; } = new();          // "PaymentService", "APIInternal"
    public string? Host { get; set; }                        // server name hoặc container ID
    
    // ===== 3. Filter Criteria =====
    public List<string> Levels { get; set; } = new List<string> { "error" };                      // "error", "warning", "info"
    public List<string>? Keywords { get; set; } = new();                  // ví dụ "TimeoutException", "JWT", "NullReference"
    public Dictionary<string, string> Tags { get; set; } = new();                    // custom tags cho Datadog, e.g. "env:prod", "team:backend"

    // ===== 4. Pagination / Query Control =====
    public int Limit { get; set; } = 5;                   // giới hạn số logs
}

