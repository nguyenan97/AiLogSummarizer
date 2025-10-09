namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters for preparing a concise manager-facing report.
/// </summary>
public sealed record ManagerReportParams : ICaseParameters
{
    /// <summary>
    /// Period of interest (ISO8601 duration or textual period resolved upstream).
    /// </summary>
    public string Period { get; init; } = string.Empty;

    /// <summary>
    /// Intended audience label (default: manager).
    /// </summary>
    public string Audience { get; init; } = "manager";

    /// <summary>
    /// Optional service scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment scope.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Whether to include SLA/SLO summary by default.
    /// </summary>
    public bool IncludeSLA { get; init; } = true;

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
