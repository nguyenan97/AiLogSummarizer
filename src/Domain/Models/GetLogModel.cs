using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Shared;

namespace Domain.Models
{
    public class GetLogModel
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public SourceType Source { get; set; } = SourceType.Datadog;
        public IntentType Intent { get; set; }
        public DesiredOutputType DesiredOutput { get; set; }
        public string? Query { get; set; }
    }
}
