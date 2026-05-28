namespace MilestoneTracker.Infrastructure.Services.BackgroundServices;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using Telegram.Bot;

public class WebhookConfigurationWorker(
    ITelegramBotClient botClient,
    IOptions<TelegramOptions> options,
    ILogger<WebhookConfigurationWorker> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var webhookUrl = options.Value.WebhookUrl;
        
        if (string.IsNullOrEmpty(webhookUrl))
        {
            logger.LogWarning("Webhook URL is empty. Skipping webhook configuration.");
            return;
        }
        
        var webhookAddress = webhookUrl.EndsWith("/api/bot") 
            ? webhookUrl 
            : $"{webhookUrl.TrimEnd('/')}/api/bot";

        logger.LogInformation("Setting Telegram webhook to: {WebhookAddress}", webhookAddress);

        try
        {
            await botClient.SetWebhook(
                url: webhookAddress,
                cancellationToken: cancellationToken);
            
            logger.LogInformation("Telegram webhook set successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set Telegram webhook to {WebhookAddress}", webhookAddress);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
