namespace MilestoneTracker.Infrastructure.Bootstrap;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Telegram.Bot;

public static class DependencyInjection
{
    public static IServiceCollection ApplyConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(
            configuration.GetSection(TelegramOptions.SectionName));

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
            new TelegramBotClient(options!.BotToken));

        return services;
    }
}