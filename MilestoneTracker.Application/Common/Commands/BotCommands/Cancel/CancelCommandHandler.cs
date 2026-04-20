namespace MilestoneTracker.Application.Common.Commands.BotCommands.Cancel;

using Domain.Enums;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;

public class CancelCommandHandler(
    IUserStateService service,
    ITelegramMessageService messageService,
    ILogger<CancelCommandHandler> logger) : IRequestHandler<CancelCommand, Unit>
{
    public async Task<Unit> Handle(CancelCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Cancel Command for Chat {ChatId}", request.ChatId);
        
        var state = await service.GetAsync(request.ChatId, cancellationToken);
            
        if (state.State != UserStateType.Idle)
        {
            await service.ResetAsync(request.ChatId, cancellationToken);
            await messageService.SendTextMessageAsync(
                request.ChatId,
                "❌ Операция отменена.",
                replyMarkup: BotKeyboards.MainMenuKeyboard,
                ct: cancellationToken);
        }
        else
        {
            await messageService.SendTextMessageAsync(
                request.ChatId,
                "Нечего отменять 🤷",
                ct: cancellationToken);
        }
       
        return Unit.Value;
    }
}