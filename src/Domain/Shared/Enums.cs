namespace Domain.Shared;

public enum IntentType
{
    Summarize,
    Analyze,
    // TODO: Add more as needed
}

public enum SourceType
{
    Datadog,
    // TODO: Add more as needed
}

public enum DesiredOutputType
{
    Text,
    Json,
    // TODO: Add more as needed
}