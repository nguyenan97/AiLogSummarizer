using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{

    public class DatadogSettings
    {
        public string Site { get; set; } = "datadoghq.com";
        public string ApiKey { get; set; } = string.Empty;
        public string AppKey { get; set; } = string.Empty;
        public string DefaultQuery { get; set; } = "status:error";
        public bool BuildQueryWithAI { get; set; }
    }

    public class LogFolderSettings
    {
        public string Path { get; set; } = string.Empty;
        public string FilePattern { get; set; } = "*.log";
        public string LogLevel { get; set; } = "Information";
    }
    public class AppInsightsSettings
    {
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string ResourceGroupName { get; set; } = string.Empty;
        public string WorkspaceName { get; set; } = string.Empty;
    }
}
