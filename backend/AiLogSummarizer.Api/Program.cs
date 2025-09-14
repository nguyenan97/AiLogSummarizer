using AiLogSummarizer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddScoped<ITextSplitter, TextSplitter>();
builder.Services.AddScoped<IChunker, SimpleChunker>();
builder.Services.AddScoped<ILogIngestionService, LogIngestionService>();
builder.Services.AddScoped<ILogSummarizer, OpenAiSummarizer>();
builder.Services.AddScoped<SlackNotifier>();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
