namespace MilestoneTracker.Infrastructure;

using Application.Common.Features.Children.AddChild;
using Application.Common.Interfaces;
using Application.Common.Shared.Interfaces;
using Application.Common.Shared.Interfaces.Repositories;
using Application.Common.Shared.Interfaces.Services;
using Application.Common.Shared.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Options;
using Persistence;
using Persistence.Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Services;
using Services.BackgroundServices;
using Telegram.Bot;

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
        
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        
        services.AddScoped<UpdateHandler>();
        services.AddSingleton<UpdateChannelQueue>();
        services.AddHostedService<UpdateWorker>();

        services.AddScoped<ITelegramMessageService, TelegramMessageService>(); 
        
        services.AddScoped<IParentRepository, ParentRepository>();
        services.AddScoped<IUserStateRepository, UserStateRepository>();
        services.AddScoped<IMilestoneRepository, MilestoneRepository>();
        
        services.AddScoped<IUserStateService, UserStateService>();
        services.AddScoped<IChildAccessTokenService, ChildAccessTokenService>();
        
        services.AddSingleton<ITelegramDateParser, TelegramDateParser>();
        
        services.AddScoped<UserFlowHandlerFactory>();
        services.AddScoped<IUserFlowHandler, ProcessChildStepHandler>();
        
        return services;
    }
    
    public static IServiceCollection ApplyConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(
            configuration.GetSection(TelegramOptions.SectionName));

        services.Configure<DatabaseOptions>(
            configuration.GetSection(DatabaseOptions.SectionName));

        return services;
    }

    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(TelegramOptions.SectionName)
            .Get<TelegramOptions>();

        if (string.IsNullOrEmpty(options.BotToken))
        {
            throw new InvalidOperationException("Telegram BotToken is missing in configuration!");
        }

        services.AddSingleton<ITelegramBotClient>(provider =>
            new TelegramBotClient(options.BotToken));

        return services;
    }

    public static IServiceCollection AddSerilogLogging(this IServiceCollection services)
    {
        return services.AddSerilog((serviceProvider, loggerConfiguration) =>
        {
            var env = serviceProvider.GetRequiredService<IHostEnvironment>();

            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Filter.ByExcluding(logEvent =>
                {
                    if (logEvent.Properties.TryGetValue("RequestPath", out var pathValue))
                    {
                        var path = pathValue.ToString().ToLower();
                        return path.Contains("/scalar") ||
                               path.Contains("/favicon") ||
                               path.EndsWith(".js\"") ||
                               path.EndsWith(".css\"");
                    }

                    return false;
                })
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MilestoneTracker.API")
                .Enrich.WithProperty("Environment", env.EnvironmentName)
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Grayscale);
        });
    }

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();

        return services;
    }
}
