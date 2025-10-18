using System.Collections.Concurrent;
using Domain.Models;
using Infrastructure.Options;
using Quartz;

namespace Infrastructure.Services.Quartz
{
    public class QuartzJobRegistry
    {
        private readonly ConcurrentDictionary<string, (JobKey jobKey, ITrigger trigger, QuartzJobConfiguration jobDetail, DateTimeOffset? lastRun)> _jobs = new();

        public IReadOnlyDictionary<string, (JobKey jobKey, ITrigger trigger, QuartzJobConfiguration jobDetail, DateTimeOffset? lastRun)> Jobs => _jobs;

        public bool TryGetJob(string name, out (JobKey jobKey, ITrigger trigger, QuartzJobConfiguration jobDetail, DateTimeOffset? lastRun) job)
            => _jobs.TryGetValue(name, out job);

        public IEnumerable<string> GetAllJobNames() => _jobs.Keys;

        public async Task<bool> AddOrUpdateJobAsync(
            IScheduler scheduler,
            string jobName,
            Type jobType,
            string cronExpression,
            Dictionary<string, string> data)
        {
            if (!typeof(IJob).IsAssignableFrom(jobType))
                throw new ArgumentException($"Type {jobType.FullName} must implement IJob.");

            var jobKey = new JobKey(jobName);

            if (_jobs.ContainsKey(jobName))
                await RemoveJobAsync(jobName, scheduler).ConfigureAwait(false);

            var jobBuilder = JobBuilder.Create(jobType)
                .WithIdentity(jobKey);

            if (data != null)
            {
                foreach (var kv in data)
                    jobBuilder.UsingJobData(kv.Key, kv.Value?.ToString());

                jobBuilder.UsingJobData("JobName", jobName);
            }

            var jobDetail = jobBuilder.Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobName}.trigger")
                .WithCronSchedule(cronExpression, x => x.WithMisfireHandlingInstructionDoNothing())
                .ForJob(jobDetail)
                .Build();

            await scheduler.ScheduleJob(jobDetail, trigger).ConfigureAwait(false);

            _jobs[jobName] = (jobKey, trigger, new QuartzJobConfiguration()
            {
                Name = jobName,
                Cron = cronExpression,
                Data = data!,
                Enabled = true,
                TypeName = jobType.Name
            }, null);
            return true;
        }

        public async Task<bool> RemoveJobAsync(string jobName, IScheduler scheduler)
        {
            if (!_jobs.TryRemove(jobName, out var jobEntry))
                return false;

            var exists = await scheduler.CheckExists(jobEntry.jobKey).ConfigureAwait(false);
            if (exists)
                await scheduler.DeleteJob(jobEntry.jobKey).ConfigureAwait(false);

            return true;
        }

        public void UpdateLastRun(string jobName, DateTimeOffset? runTime)
        {
            if (_jobs.TryGetValue(jobName, out var entry))
                _jobs[jobName] = (entry.jobKey, entry.trigger, entry.jobDetail, runTime);
        }

        public async Task PauseAllAsync(IScheduler scheduler)
            => await scheduler.PauseAll();

        public async Task ResumeAllAsync(IScheduler scheduler)
            => await scheduler.ResumeAll();

        public async Task RunAllAsync(IScheduler scheduler)
        {
            foreach (var job in _jobs.Values)
                await scheduler.TriggerJob(job.jobKey);
        }
    }
}