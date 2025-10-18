using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.MentionParsing.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntentType
{
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    //Check errors theo range
    // 🔍 **Summarize** — Tóm tắt lỗi, log, hoặc trạng thái tổng quan trong khoảng thời gian.
    // - Bao gồm: các truy vấn về lỗi gần nhất, lỗi hôm nay, tuần này, tháng này.
    // - Ví dụ: “Cho tôi lỗi hôm nay”, “5 lỗi gần nhất”, “Tổng hợp lỗi theo service”.
    [EnumMember(Value = "summarize")]
    Summarize,

    //Detail error
    // 🧠 **Analyze** — Phân tích chi tiết nguyên nhân sự cố, trace, exception, hoặc vấn đề bảo mật.
    // - Ví dụ: “Phân tích root cause của lỗi 504”, “Tìm traceId abc123”, “NullReference ở order-svc hôm nay”, “Phát hiện chậm rò rỉ bộ nhớ”.
    [EnumMember(Value = "analyze")]
    Analyze,

    //Daily report
    // 📊 **Report** — Tạo báo cáo định kỳ hoặc tổng hợp cho stakeholder (manager, SLA report, alert summary...).
    // - Ví dụ: “Tạo báo cáo daily report”, “Gửi báo cáo tuần cho manager”, “Tổng hợp SLA tháng này”, “Cảnh báo tạm thời cho dashboard”.
    [EnumMember(Value = "report")]
    Report,

    //[EnumMember(Value = "alert")]
    //Alert,

    //[EnumMember(Value = "investigate")]
    //Investigate
}
