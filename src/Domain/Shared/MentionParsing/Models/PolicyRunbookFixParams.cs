namespace Domain.MentionParsing.Models;

/// <summary>
/// Parameters to generate suggested fixes based on policies/runbooks.
/// </summary>
public sealed record PolicyRunbookFixParams : ICaseParameters
{
    /// <summary>
    /// Short issue summary in the user's words.
    /// </summary>
    public string Issue { get; init; } = string.Empty;

    /// <summary>
    /// Severity hint (default: medium). Values like low/medium/high/critical.
    /// </summary>
    public string Severity { get; init; } = "medium";

    /// <summary>
    /// Optional service scope.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>
    /// Optional environment scope.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
