using Application.Common.Interfaces;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.MentionParsing.Models;
using Domain.Models;
using Domain.Shared;
using Infrastructure.AI.MentionParser;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.LogSources;
using Infrastructure.Slack;
using LogReader.Services.Sources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddOptions<DatadogSettings>().Bind(configuration.GetSection("Datadog"));
        services.AddOptions<LogFolderSettings>().Bind(configuration.GetSection("LogFolder"));

        services.AddSlackNet(c =>
        {
            c.UseApiToken(configuration["Slack:BotToken"]!);
            c.UseSigningSecret(configuration["Slack:SigningSecret"]!);
            c.RegisterEventHandler<AppMention>(ctx => ctx.ServiceProvider().GetRequiredService<AppMentionEventHandler>());
        });

        // AppMention handler DI
        services.AddScoped<AppMentionEventHandler>();

        // Slack chat service (SlackNet added in WebApi)
        services.AddScoped<ISlackChatService, SlackChatService>();

        // Other services
        services.AddScoped<IMentionParserService, MentionParserService>();
        services.AddScoped<ISummarizerService, SummarizerService>();
        services.AddKeyedSingleton<ILogSourceService, DatadogLogSource>(SourceType.Datadog);
        services.AddKeyedSingleton<ILogSourceService, FolderLogSource>(SourceType.Folder);
        services.AddSingleton<ICompositeLogSource, CompositeLogSource>();

        return services;
    }
}