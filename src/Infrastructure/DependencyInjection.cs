using System;
using Application.Common.Interfaces;
using Application.Interfaces;
using Azure;
using Azure.AI.OpenAI;
using Domain.Interfaces;
using Domain.Models;
using Domain.Shared;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.LogSources;
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

        services.AddOptions<AzureOpenAIOptions>()
            .Bind(configuration.GetSection("OpenAI"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<DatadogSettings>().Bind(configuration.GetSection("Datadog"));
        services.AddOptions<LogFolderSettings>().Bind(configuration.GetSection("LogFolder"));


        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
            return new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey));
        });

        // SlackNet: tokens + signing secret + event handlers
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

