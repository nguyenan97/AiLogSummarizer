namespace Domain.Shared;

public record TraceLog(DateTimeOffset Timestamp, string Message, string Level, string Source);
