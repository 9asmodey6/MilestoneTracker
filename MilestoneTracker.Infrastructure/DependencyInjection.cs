namespace MilestoneTracker.Infrastructure;

using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Persistence;
using Persistence.Repositories;
using Services;
using Services.BackgroundServices;

public static class DependencyInjection
{
    public static IServiceCollection AddInfractructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>();

        if (string.IsNullOrEmpty(dbOptions?.ConnectionString))
        {
            throw new InvalidOperationException("Connection String is missing in configuration!");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dbOptions.ConnectionString)
                .UseSnakeCaseNamingConvention());
        
        services.AddScoped<UpdateHandler>();

        services.AddScoped<ITelegramMessageService, TelegramMessageService>(); 
        services.AddScoped<IParentRepository, ParentRepository>();
        
        services.AddSingleton<UpdateChannelQueue>();
        services.AddHostedService<UpdateWorker>();
        
        return services;
    }
}