namespace Domain.Shared;

public record TraceLog(DateTime Timestamp, string Message, string Level, string Source);