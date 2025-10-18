using Application.Features.Mentions;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Services.Quartz.Jobs
{
    public class LogReportJob : IJob
    {
        private readonly ILogger<LogReportJob> _logger;
        private readonly IMediator _mediator;

        public LogReportJob(ILogger<LogReportJob> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobName = context.JobDetail.JobDataMap.GetString("JobName") ?? string.Empty;
            var channelId = context.JobDetail.JobDataMap.GetString("ChannelId") ?? string.Empty;
            var prompt = context.JobDetail.JobDataMap.GetString("Prompt") ?? string.Empty;

            _logger.LogInformation("[{Instance} = {Key}] executing...", nameof(LogReportJob), jobName);
            _logger.LogInformation("{Channel} - {Prompt}", channelId, prompt);

            await _mediator.Send(new HandleAppMentionCommand(prompt, channelId, string.Empty)).ConfigureAwait(true);
        }
    }
}
