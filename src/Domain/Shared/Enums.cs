namespace Domain.Shared;

public enum IntentType
{
    Summarize,
    Analyze,
    Report
}

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
