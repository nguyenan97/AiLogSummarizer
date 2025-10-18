using Quartz;

namespace Infrastructure.Services.Quartz
{
    public class QuartzJobListener : IJobListener
    {
        private readonly QuartzJobRegistry _registry;

        public QuartzJobListener(QuartzJobRegistry registry)
        {
            _registry = registry;
        }

        public string Name => nameof(QuartzJobListener);

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
        {
            _registry.UpdateLastRun(context.JobDetail.Key.Name, DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}