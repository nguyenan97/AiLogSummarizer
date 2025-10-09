using Application.Common.Interfaces;
using Application.Interfaces;
using Azure;
using Azure.AI.OpenAI;
using Domain.Interfaces;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.Providers;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.LogSources;
using Infrastructure.Services.MentionParser;
using Infrastructure.Services.Summarizer;
using Infrastructure.Slack;
using LogReader.Services.Sources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SlackNet.AspNetCore;
using SlackNet.Events;
using SlackNet.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IDateTimeService, DateTimeService>();

        services.AddOptions<MentionParserOptions>()
            .Bind(configuration.GetSection("MentionParser"))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Endpoint) &&
                !string.IsNullOrWhiteSpace(options.DeploymentName),
                "MentionParser configuration requires both Endpoint and DeploymentName.")
            .ValidateOnStart();


        services.AddOptions<AiProcessingOptions>().Bind(configuration.GetSection("AiProcessing"));
        services.AddOptions<DatadogSettings>().Bind(configuration.GetSection("Datadog"));
        services.AddOptions<LogFolderSettings>().Bind(configuration.GetSection("LogFolder"));

        //Đăng ký SummarizerProvider
        services.AddScoped<SummarizerProvider>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
            return new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey));
        });
        var senderServiceName = configuration.GetSection("SenderService")?.Value ?? "Slack";
        switch (senderServiceName)
        {
            case "Slack":
                {
                    services.AddSlackNet(c =>
                    {
                        c.UseApiToken(configuration["Slack:BotToken"]!);
                        c.UseSigningSecret(configuration["Slack:SigningSecret"]!);
                        c.RegisterEventHandler<AppMention>(ctx => ctx.ServiceProvider().GetRequiredService<AppMentionEventHandler>());
                    });
                    services.AddScoped<IMessageSenderService, SlackChatService>();
                    services.AddScoped<AppMentionEventHandler>();
                }
                break;
            case "Console":
                services.AddScoped<IMessageSenderService, ConsoleChatService>();
                break;
        }

        // Other services
        services.AddScoped<IMentionParserService, MentionParserService>();
        services.AddKeyedSingleton<ILogSourceService, DatadogLogSource>(SourceType.Datadog);
        services.AddKeyedSingleton<ILogSourceService, FolderLogSource>(SourceType.Folder);
        services.AddSingleton<ICompositeLogSource, CompositeLogSource>();
        services.AddScoped<ISummarizerService, SummarizerService>();


        return services;
    }
}
