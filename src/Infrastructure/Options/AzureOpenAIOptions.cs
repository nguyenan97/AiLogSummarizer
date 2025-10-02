using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public sealed class AzureOpenAIOptions
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string DeploymentName { get; set; } = string.Empty;
}
