using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class GeneratedError
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public int? Code { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}
