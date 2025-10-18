namespace Domain.Shared;

public class TraceLog
{
    public DateTimeOffset Timestamp { get; set; }
    public string Message { get; set; }
    public string Level { get; set; }
    public string Source { get; set; }
    public string TraceId { get; set; }
    public string FullLog { get; set; }

    public TraceLog(DateTimeOffset timestamp, string message, string level, string source, string traceId, string fullLog)
    {
        Timestamp = timestamp;
        Message = message;
        Level = level;
        Source = source;
        TraceId = traceId;
        FullLog = fullLog;
    }
}

