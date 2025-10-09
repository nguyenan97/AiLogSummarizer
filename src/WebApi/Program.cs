using Application;
using Infrastructure;
using SlackNet.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

var app = builder.Build();

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
