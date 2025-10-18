
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Infrastructure.Services.FakeLogs
{
    public class FakeLogBackgroundService : BackgroundService
    {
        private readonly ILogger<FakeLogBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public FakeLogBackgroundService(ILogger<FakeLogBackgroundService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FakeLogBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await GenerateFakeLog();
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            _logger.LogInformation("FakeLogBackgroundService is stopping.");
        }

        private async Task GenerateFakeLog()
        {
            string requestId = Guid.NewGuid().ToString();
            Log.Information("Starting request processing for {RequestId}", requestId);
            RequestSimulator.LogRandomRequest();
            // 2% chance (1/50) to trigger errors
            Random random = new Random();
            bool triggerError = random.NextDouble() < 0.02; // 1/50 = 0.02

            if (triggerError)
            {
                Log.Information("Triggering error simulation for request {RequestId}", requestId);
                await ErrorLogSimulator.SimulateRandomErrors(_configuration);

            }
            else
            {
                Log.Information("Request {RequestId} completed successfully with no errors.", requestId);

            }

        }


    }
}
