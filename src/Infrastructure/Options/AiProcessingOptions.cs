namespace Infrastructure.Options
{
    /// <summary>
    /// Lớp cha chứa toàn bộ cấu hình xử lý AI.
    /// </summary>
    public class AiProcessingOptions
    {
        public TaskSettings TaskSettings { get; set; } = new();
        public string DefaultProvider { get; set; } = string.Empty;
        public List<ProviderConfig> Providers { get; set; } = new();
    }

    /// <summary>
    /// Cấu hình cho từng tác vụ cụ thể.
    /// </summary>
    public class TaskSettings
    {
        public string? ChunkProcessorProvider { get; set; }
        public string? MergeProcessorProvider { get; set; }
    }

    /// <summary>
    /// Đại diện cho một cấu hình của một nhà cung cấp AI.
    /// </summary>
    public class ProviderConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ProviderSettings Settings { get; set; } = new();
    }

    /// <summary>
    /// Chứa tất cả các thuộc tính cài đặt có thể có từ mọi provider.
    /// </summary>
    public class ProviderSettings
    {
        public string ModelNameForTokenCount { get; set; } = string.Empty;
        public int ModelTokenLimit { get; set; } = 4096;
        public int SafeMargin { get; set; } = 1024;
        public string? Endpoint { get; set; }
        public string? ApiKey { get; set; }
        public string? DeploymentName { get; set; }
        public string? ModelName { get; set; }
    }
}
