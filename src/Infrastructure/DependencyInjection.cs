using System.Text;
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
using Infrastructure.Services.FakeLogs;
using Infrastructure.Services.LogSources;
using Infrastructure.Services.Mem0;
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

        services.AddOptions<Mem0Options>()
            .Bind(configuration.GetSection("Mem0"))
            //.Validate(options =>
            //    !string.IsNullOrWhiteSpace(options.ApiKey),
            //    "Mem0 configuration requires ApiKey.")
            //.Validate(options =>
            //    !string.IsNullOrWhiteSpace(options.BaseUrl),
            //    "Mem0 configuration requires BaseUrl.")
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

        // Register Mem0Client
        services.AddSingleton<IMem0Client>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<Mem0Options>>().Value;
            return new Mem0Client(options.ApiKey, options.BaseUrl);
        });

        services.AddSingleton<ILocalMem0Client>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<Mem0Options>>().Value;
            return new LocalMem0Client(options.BaseUrl);
        });

        // Register HistoryLayerService
        services.AddScoped<IHistoryLayerService, LocalHistoryLayerService>();

        // SenderService configuration
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
                {
                    Console.OutputEncoding = Encoding.UTF8;
                    services.AddScoped<IMessageSenderService, ConsoleChatService>();
                }
           
                break;
        }
        if (configuration.GetValue<bool>("RunGenrateFakeLog") == true)
        {
            services.AddHostedService<FakeLogBackgroundService>();
        }
        services.AddScoped<IFakeLogGenerateService, FakeLogGenerateService>();
        // Other services
        services.AddScoped<IMentionParserService, MentionParserService>();
        services.AddKeyedScoped<ILogSourceService, DatadogLogSource>(SourceType.Datadog);
        services.AddKeyedScoped<ILogSourceService, FolderLogSource>(SourceType.Folder);
        services.AddScoped<ICompositeLogSource, CompositeLogSource>();
        services.AddScoped<ISummarizerService, SummarizerService>();


        return services;
    }
}
