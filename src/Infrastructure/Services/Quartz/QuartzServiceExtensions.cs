using Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Services.Quartz
{
    public static class QuartzServiceExtensions
    {
        public static IServiceCollection AddOopsAIQuarzt(this IServiceCollection services, QuartzConfiguration quartzConfiguration)
        {
            ArgumentNullException.ThrowIfNull(quartzConfiguration);

            if (quartzConfiguration.Enabled)
            {
                services.AddSingleton<QuartzJobRegistry>();
                services.AddSingleton<IJobListener, QuartzJobListener>();

                services.AddQuartz(q =>
                {
                    q.UseInMemoryStore();
                    //q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);
                });

                services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

                if (quartzConfiguration.Jobs.Count > 0)
                {
                    services.AddSingleton(quartzConfiguration.Jobs);
                }
            }

            return services;
        }

        public static async Task InitializeConfiguredJobsAsync(this IServiceProvider provider)
        {
            var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
            var registry = provider.GetRequiredService<QuartzJobRegistry>();
            var jobConfigs = provider.GetService<List<QuartzJobConfiguration>>();

            var scheduler = await schedulerFactory.GetScheduler();
            scheduler.ListenerManager.AddJobListener(provider.GetRequiredService<IJobListener>());

            if (jobConfigs == null) return;

            foreach (var cfg in jobConfigs)
            {
                var jobType = Type.GetType(cfg.TypeName, throwOnError: false);
                if (jobType == null) continue;
                if (cfg.Enabled)
                {
                    await registry.AddOrUpdateJobAsync(scheduler, cfg.Name, jobType, cfg.Cron, cfg.Data).ConfigureAwait(false);
                }
            }
        }
    }
}
