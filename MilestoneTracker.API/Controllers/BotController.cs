namespace MilestoneTracker.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Infrastructure.Services;
using System.Text.Json;
using Infrastructure.Services;

[ApiController]
[Route("api/bot")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly UpdateHandler _updateHandler;

    public WebhookController(ILogger<WebhookController> logger, UpdateHandler updateHandler)
    {
        _logger = logger;
        _updateHandler = updateHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken ct)
    {
        _logger.LogInformation("---> [WEBHOOK] Получено обновление ID: {UpdateId}, Тип: {UpdateType}", 
            update.Id, update.Type);

        try
        {
            await _updateHandler.HandleUpdateAsync(update, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " Ошибка при обработке Update {UpdateId}", update.Id);
        }
        
        
        return Ok();
    }

    [HttpGet]
    public string Get() => "Узел связи MilestoneTracker активен! 🫡";
}