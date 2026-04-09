namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Bot.Comands.Start;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Telegram.Bot.Types;

public class UpdateHandler(
    IMediator mediator,
    ILogger<UpdateHandler> logger)
    //IUserStateManager stateManager)
{
    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        var context = BotContext.FromUpdate(update);
        if (context == null)
        {
            return;
        }

        logger.LogInformation("Processing update for ChatId: {ChatId}", context.ChatId);

        await HandleCommand(context, ct);
    }

    private async Task HandleCommand(BotContext context, CancellationToken ct)
    {
        switch (context.Text)
        {
            case "/start":
                await mediator.Send(new StartCommand(
                    context.ChatId, 
                    context.FirstName, 
                    context.Username), ct);
                break;

            default:
                throw new InvalidOperationException();
                // await HandleUnknownCommand(context.ChatId, ct);
                break;
        }
    }
}