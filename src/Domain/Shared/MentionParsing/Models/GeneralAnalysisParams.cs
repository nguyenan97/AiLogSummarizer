using System.Collections.Generic;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Generic fallback capturing detected intent hints and key fields.
/// </summary>
public sealed record GeneralAnalysisParams : ICaseParameters
{
    /// <summary>
    /// A short hint of what the user seems to want.
    /// </summary>
    public string DetectedIntent { get; init; } = string.Empty;

    /// <summary>
    /// Important extracted fields as a flat map (snake_case keys).
    /// </summary>
    public Dictionary<string, string> Fields { get; init; } = new();

    /// <summary>
    /// Optional guidance or follow-up notes.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
