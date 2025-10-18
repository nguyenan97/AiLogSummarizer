using System;
using System.Collections.Generic;

namespace Domain.MentionParsing.Models;

public sealed record MentionParserOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string DeploymentName { get; init; } = string.Empty;
    public string? ApiKey { get; init; }
    public string TimeZone { get; init; } = "UTC";

    public IReadOnlyList<string> KnownServices { get; init; } = new List<string> { "product-api", "order-api", "user-api", "inventory-api", "payment-api", "shipping-api" };
    public IReadOnlyList<string> KnownEnvironments { get; init; } = new List<string> { "qc", "uat", "staging", "production" };
}
