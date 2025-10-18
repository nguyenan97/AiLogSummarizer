using Application;
using Infrastructure;
using Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Diagnostics;
using SlackNet.AspNetCore;
using Infrastructure.Services.Quartz;
using Infrastructure.Options;

// Bootstrap logger (captures early startup errors)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/bootstrap-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, shared: true)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();
var configuration = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .Build();



var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog
builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName), writeToProviders: true);

builder.AddServiceDefaults();

var quartzConfiguration = configuration.GetSection(QuartzConfiguration.SectionName).Get<QuartzConfiguration>();
builder.Services.AddOopsAIQuarzt(quartzConfiguration!);

builder.Configuration
    .AddUserSecrets(typeof(Program).Assembly)
    .AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "API Documentation"
    });
});

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

// RFC7807 ProblemDetails service
builder.Services.AddProblemDetails();
// Register custom exception handler using built-in diagnostics
builder.Services.AddExceptionHandler<AppExceptionHandler>();

var app = builder.Build();

// Global exception handling middleware (built-in)
app.UseExceptionHandler();

// CorrelationId + TraceId enrichment for Serilog LogContext
app.Use(async (context, next) =>
{
    var cid = context.Request.Headers.TryGetValue("X-Correlation-Id", out var incoming) && !string.IsNullOrWhiteSpace(incoming)
        ? incoming.ToString()
        : Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Correlation-Id"] = cid;

    var activityTraceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", cid))
    using (Serilog.Context.LogContext.PushProperty("TraceId", activityTraceId))
    {
        await next();
    }
});

// Serilog request logging with useful enrichers
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagCtx, http) =>
    {
        var activityTraceId = Activity.Current?.TraceId.ToString() ?? http.TraceIdentifier;
        diagCtx.Set("TraceId", activityTraceId);
        if (http.Request.Headers.TryGetValue("X-Correlation-Id", out var cid))
        {
            diagCtx.Set("CorrelationId", cid.ToString());
        }
    };
});

await app.Services.InitializeConfiguredJobsAsync().ConfigureAwait(false);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    c.RoutePrefix = string.Empty;
    c.HeadContent = @"<script src=""https://cdnjs.cloudflare.com/ajax/libs/crypto-js/4.1.1/crypto-js.min.js""></script>
<script>
window.addEventListener('load', function() {
    var modal = document.createElement('div');
    modal.id = 'passwordModal';
    modal.innerHTML = `<div style=""background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);"">
        <h3 style=""margin: 0 0 15px 0; color: #333;"">Swagger Access</h3>
        <p style=""margin: 0 0 15px 0; color: #666;"">Enter password (123 for testing this feature):</p>
        <input type=""password"" id=""passwordInput"" style=""width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid #ccc; border-radius: 5px; font-size: 16px;"">
        <button id=""submitBtn"" style=""width: 100%; padding: 10px; background: #007bff; color: white; border: none; border-radius: 5px; font-size: 16px; cursor: pointer;"">Submit</button>
    </div>`;
    modal.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: black; display: flex; justify-content: center; align-items: center; z-index: 1000;';
    document.body.appendChild(modal);
    document.getElementById('submitBtn').addEventListener('click', function() {
        var password = document.getElementById('passwordInput').value;
        var hash = CryptoJS.SHA256(password).toString();
        if (hash === 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3') {
            modal.style.display = 'none';
        } else {
            alert('Incorrect password');
        }
    });
});
</script>";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
if ((app.Configuration["SenderService"] ?? "Slack") == "Slack")
{
    // Slack events endpoint (default route prefix: /slack)
    app.UseSlackNet();
}


app.MapHealthChecks("/health");

app.MapDefaultEndpoints();

await app.RunAsync();
