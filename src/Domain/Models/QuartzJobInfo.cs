namespace Domain.Models
{
    public class QuartzJobInfo
    {
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Cron { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }
}
