using System;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Fallback parameters when the router cannot determine a precise intent.
/// </summary>
public sealed record UnknownIntentParams : ICaseParameters
{
    /// <summary>
    /// A few example prompts to educate the user.
    /// </summary>
    public string[] Samples { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Optional notes with guidance in Vietnamese.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Common Datadog-friendly context (service/env/time/tags/trace).
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
