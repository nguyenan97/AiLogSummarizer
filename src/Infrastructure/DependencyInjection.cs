using Application.Common.Interfaces;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using SlackNet.AspNetCore;
using SlackNet.Events;
using Infrastructure.Slack;
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
        services.AddScoped<ILogSourceService, LogSourceService>();
        services.AddScoped<ISummarizerService, SummarizerService>();

        return services;
    }
}
