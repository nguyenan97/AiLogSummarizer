using System;
using System.Collections.Generic;

namespace Domain.MentionParsing.Models;

public sealed record MentionParserOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string DeploymentName { get; init; } = string.Empty;
    public string? ApiKey { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}