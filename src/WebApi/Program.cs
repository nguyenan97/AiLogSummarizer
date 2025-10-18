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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    c.RoutePrefix = string.Empty;
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
