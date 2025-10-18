using Domain.Models;
using Infrastructure.Services.Quartz;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace WebApi.Controllers;

[ApiController]
public class QuartzController : ControllerBase
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly QuartzJobRegistry _registry;

    public QuartzController(ISchedulerFactory schedulerFactory, QuartzJobRegistry registry)
    {
        _schedulerFactory = schedulerFactory;
        _registry = registry;
    }

    [HttpGet("api/schedules")]
    public async Task<IActionResult> GetAllJobs()
    {
        var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
        var jobs = _registry.Jobs.Select(x => new
        {
            JobName = x.Key,
            Trigger = x.Value.trigger.Key.Name,
            x.Value.jobDetail.Cron,
            JobType = x.Value.jobDetail.TypeName,
            Metadata = x.Value.jobDetail.Data,
            LastRun = x.Value.lastRun?.LocalDateTime,
            NextRun = x.Value.trigger.GetFireTimeAfter(x.Value.lastRun?.LocalDateTime)?.LocalDateTime
        });
        return Ok(jobs);
    }

    [HttpPost("api/schedules")]
    public async Task<IActionResult> AddJob([FromBody] QuartJobRequest request)
    {
        var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
        var jobType = Type.GetType(request.TypeName, throwOnError: false);

        if (jobType == null)
            return BadRequest($"Cannot load type: {request.TypeName}");

        await _registry.AddOrUpdateJobAsync(scheduler, request.Name, jobType, request.Cron, request.Data).ConfigureAwait(false);
        return Ok($"Job {request.Name} added/updated successfully.");
    }

    [HttpPut("api/schedules")]
    public async Task<IActionResult> UpdateJob([FromBody] QuartJobRequest request)
    {
        var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
        var jobType = Type.GetType(request.TypeName, throwOnError: false);

        if (jobType == null)
            return BadRequest($"Cannot load type: {request.TypeName}");

        await _registry.AddOrUpdateJobAsync(scheduler, request.Name, jobType, request.Cron, request.Data).ConfigureAwait(false);
        return Ok($"Job {request.Name} updated successfully.");
    }

    [HttpDelete("api/schedules/{jobName}")]
    public async Task<IActionResult> RemoveJob(string jobName)
    {
        var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
        var success = await _registry.RemoveJobAsync(jobName, scheduler).ConfigureAwait(false);
        return success ? Ok($"Job {jobName} removed.") : NotFound($"Job {jobName} not found.");
    }

    [HttpPost("api/schedules/{jobName}/run")]
    public async Task<IActionResult> RunJob(string jobName)
    {
        var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
        if (_registry.TryGetJob(jobName, out var job))
        {
            await scheduler.TriggerJob(job.jobKey).ConfigureAwait(false);
            return Ok($"Job {jobName} triggered manually.");
        }
        return NotFound($"Job {jobName} not found.");
    }
}