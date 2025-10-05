using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.MentionParsing.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntentCategory
{
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    [EnumMember(Value = "summarize")]
    Summarize,

    [EnumMember(Value = "analyze")]
    Analyze,

    [EnumMember(Value = "report")]
    Report,

    [EnumMember(Value = "alert")]
    Alert,

    [EnumMember(Value = "investigate")]
    Investigate
}