namespace Domain.Shared;

public enum SourceType
{
    Datadog,
    Folder
}

public enum DesiredOutputType
{
    Text,
    Json
}

public enum ProcessingStrategy
{
    /// <summary>
    /// Xử lý từng chunk một cách tuần tự. Chậm nhất, ổn định nhất.
    /// </summary>
    Sequential,

    /// <summary>
    /// Xử lý nhiều chunk song song. Nhanh hơn Sequential nhưng tốn nhiều API call.
    /// </summary>
    Parallel,

    /// <summary>
    /// Gom nhiều chunk vào một API call duy nhất. Nhanh và hiệu quả nhất.
    /// </summary>
    Batched
}