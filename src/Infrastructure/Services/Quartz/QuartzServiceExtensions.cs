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
                services.AddQuartz(q =>
                {
                    q.UseInMemoryStore();

                    foreach (var quartzJob in quartzConfiguration.Jobs)
                    {
                        if (string.IsNullOrEmpty(quartzJob.Name)
                            || string.IsNullOrEmpty(quartzJob.TypeName)
                            || !quartzJob.Enabled)
                        {
                            continue;
                        }

                        var jobType = Type.GetType(quartzJob.TypeName, throwOnError: false);
                        if (jobType is null || !typeof(IJob).IsAssignableFrom(jobType))
                        {
                            continue;
                        }

                        var jobKey = new JobKey(quartzJob.Name);

                        q.AddJob(jobType, jobKey, (opts) =>
                        {
                            if (quartzJob.Data.Count > 0)
                            {
                                foreach (var item in quartzJob.Data)
                                {
                                    opts.UsingJobData(item.Key, item.Value);
                                }
                            }
                        });
                        q.AddTrigger(opts => opts
                            .ForJob(jobKey)
                            .WithIdentity($"{quartzJob.Name}.trigger")
                            .WithCronSchedule(quartzJob.Cron, x => x.WithMisfireHandlingInstructionDoNothing())
                        );
                    }
                });

                services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            }

            return services;
        }
    }
}
