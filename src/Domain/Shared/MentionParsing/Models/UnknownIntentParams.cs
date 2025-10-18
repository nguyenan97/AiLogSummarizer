using System;

namespace Domain.MentionParsing.Models;

/// <summary>
/// Minimal unknown-intent payload: a single concise message produced by the AI.
/// </summary>
public sealed record UnknownIntentParams : ICaseParameters
{
    /// <summary>
    /// A concise, user-facing message (Markdown allowed). Language is chosen by the AI.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Keep required context to satisfy ICaseParameters.
    /// </summary>
    public CaseContext Context { get; init; } = new();
}
