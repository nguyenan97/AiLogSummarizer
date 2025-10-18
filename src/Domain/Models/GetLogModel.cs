using Domain.MentionParsing.Models;
using Domain.Shared;

namespace Domain.Models;

public class LogQueryContext
{
    public LogQueryContext()
    {
        
    }
    //public LogQueryContext(MentionParsed mentionParsed)
    //{
    //    var context = mentionParsed?.Parameters?.Context;
    //    if(context==null)
    //        throw new ArgumentNullException(nameof(context));
    //    From = context.FromIso;
    //    To = context.ToIso;
    //    Environment= context.Environment;
    //    ServiceNames = new List<string>();
    //    if (context.Service != null)
    //    {
    //        ServiceNames.Add(context.Service);
    //    }
    //    else if(context.Services!=null)
    //    {
    //        ServiceNames.AddRange(context.Services);
    //    }
    //    if (context.Tags != null)
    //    {
    //        Tags = context.Tags.ToDictionary();

    //    }
      
    //    Limit = context.Limit ?? 5;
       
    //}

    // ===== 1. Basic Context =====
    public SourceType Source { get; set; } = SourceType.Datadog; // "datadog", "application-insights", "elk", v.v.
    public string? From { get; set; }
    public string? To { get; set; }

    // ===== 2. Environment/Scope =====
    public string? Environment { get; set; }                 // "prod", "staging", "dev"
    public List<string> ServiceNames { get; set; } =  new();          // "PaymentService", "APIInternal"
    public string? Host { get; set; }                        // server name hoặc container ID
    //public string? Application { get; set; }                 // nếu có nhiều app
    //public string? Region { get; set; }                      // "us-east", "ap-southeast-1" ...

    // ===== 3. Filter Criteria =====
    public List<string> Levels { get; set; } = new List<string> { "error" };                      // "error", "warning", "info"
    public List<string>? Keywords { get; set; }   = new ();                  // ví dụ "TimeoutException", "JWT", "NullReference"
    public Dictionary<string, string> Tags { get; set; } = new();                    // custom tags cho Datadog, e.g. "env:prod", "team:backend"

    // ===== 4. Pagination / Query Control =====
    public int Limit { get; set; } = 5;                   // giới hạn số logs
    //public int? Offset { get; set; }                         // nếu có paging
    //public bool IncludeTraces { get; set; } = false;         // kèm trace logs?

    //// ===== 5. Auth / API Config =====
    //public string? ApiKey { get; set; }                      // có thể override per user/session
    //public string? ProjectId { get; set; }                   // nếu source là GCP / Datadog org

    //public IntentType Intent { get; set; }
    //public DesiredOutputType DesiredOutput { get; set; }
    //public string? Query { get; set; }
    //public string? TraceId { get; set; }
}