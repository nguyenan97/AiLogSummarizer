using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.MentionParsing.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntentType
{
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    //Check errors theo range
    [EnumMember(Value = "summarize")]
    Summarize,

    //Detail error
    [EnumMember(Value = "analyze")]
    Analyze,

    //Daily report
    [EnumMember(Value = "report")]
    Report,

    //[EnumMember(Value = "alert")]
    //Alert,

    //[EnumMember(Value = "investigate")]
    //Investigate
}
