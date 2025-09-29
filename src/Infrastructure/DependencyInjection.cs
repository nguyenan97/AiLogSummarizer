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

        // Slack services
        services.AddSlackNet(c => c.UseApiToken(configuration["Slack:BotToken"]));
        services.AddScoped<ISlackChatService, SlackChatService>();

        // Other services
        services.AddScoped<IMentionParserService, MentionParserService>();
        services.AddScoped<ILogSourceService, LogSourceService>();
        services.AddScoped<ISummarizerService, SummarizerService>();

        return services;
    }
}