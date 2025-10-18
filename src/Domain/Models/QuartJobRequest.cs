namespace Domain.Models
{
    public class QuartJobRequest
    {
        public string Name { get; set; } = default!;
        public string TypeName { get; set; } = default!;
        public string Cron { get; set; } = default!;
        public Dictionary<string, string> Data { get; set; } = [];
    }
}
