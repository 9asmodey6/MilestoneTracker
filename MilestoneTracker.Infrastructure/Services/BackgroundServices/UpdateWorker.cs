namespace MilestoneTracker.Infrastructure.Services.BackgroundServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

public class UpdateWorker(
    UpdateChannelQueue queue, 
    IServiceScopeFactory scopeFactory,
    ILogger<UpdateWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Update Worker started");

        await foreach (var update in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
                
                await  handler.HandleUpdateAsync(update, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process background update {UpdateId}", update.Id);
            }
        }
    }
}