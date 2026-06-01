namespace MilestoneTracker.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Infrastructure.Services;
using System.Text.Json;
using Application.Common.Constants;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[EnableRateLimiting(PoliciesConstants.RateLimiterPolicies.TelegramBotPolicy)]
[Route(EndpointConstants.TelegramBotRoute)]
public class WebhookController(ILogger<WebhookController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateChannelQueue queue,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Update ID: {UpdateId}, Type: {UpdateType}",
            update.Id,
            update.Type);

        try
        {
            await queue.PushUpdateAsync(update, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while pushing Update {UpdateId} to the queue", update.Id);
        }

        return Ok();
    }
}