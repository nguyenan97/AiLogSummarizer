using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.MentionParsing.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InputLanguage
{
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    [EnumMember(Value = "en")]
    [Description("English")]
    English,

    [EnumMember(Value = "vi")]
    [Description("Vietnamese")]
    Vietnamese,
}

