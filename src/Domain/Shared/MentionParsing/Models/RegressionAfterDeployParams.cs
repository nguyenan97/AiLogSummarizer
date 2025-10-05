namespace Domain.MentionParsing.Models;

public sealed record RegressionAfterDeployParams : ICaseParameters
{
    public string DeployMarker { get; init; } = string.Empty;
    public string LookbackBefore { get; init; } = "PT24H";
    public string LookbackAfter { get; init; } = "PT24H";
    public string? Service { get; init; }
    public string? Environment { get; init; }
    public string TimeZone { get; init; } = "Asia/Ho_Chi_Minh";
}
