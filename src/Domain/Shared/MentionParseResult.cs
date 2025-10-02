using System.Text.Json.Serialization;

namespace Domain.Shared;

public sealed record MentionParseRange
{
    [JsonPropertyName("start")]
    public string? Start { get; init; }

    [JsonPropertyName("end")]
    public string? End { get; init; }
}

public sealed record MentionParseResult
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("intent")]
    public string? Intent { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("range")]
    public MentionParseRange? Range { get; init; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; init; }

    [JsonPropertyName("suggestion")]
    public string? Suggestion { get; init; }
}
