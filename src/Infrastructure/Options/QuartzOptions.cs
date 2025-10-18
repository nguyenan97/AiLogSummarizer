namespace Infrastructure.Options
{
    public sealed class QuartzConfiguration
    {
        public const string SectionName = "QuartzOptions";

        public required bool Enabled { get; set; }

        public List<QuartzJobConfiguration> Jobs { get; set; } = [];
    }

    public sealed record QuartzJobConfiguration
    {
        public required string Name { get; set; }

        public required string TypeName { get; set; }

        public required string Cron { get; set; }

        public required bool Enabled { get; set; }

        public required Dictionary<string, string> Data { get; set; } = [];
    }
}