namespace MilestoneTracker.Application.Common.Features.Children.GainAccess.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Models;
using Shared.State;

public class EnteringGainStepHandler(
    ITelegramMessageService messageService,
    ILogger<EnteringGainStepHandler> logger) : IStepHandler<GainAccessByTokenCommand>
{
    public UserStateType Step => UserStateType.GainAccessEnteringToken;

    public async Task<StepResult<GainAccessByTokenCommand>> HandleAsync(BotContext context, GainAccessByTokenCommand data, CancellationToken ct)
    {
        logger.LogInformation("Parent {ChatId} started gaining access by token flow.", context.ChatId);

        await messageService.SendTextMessageAsync(
            context.ChatId,
            "🔑 <b>Введите код доступа</b>\n\n" +
            "Пожалуйста, пришлите специальный код, который сгенерировал второй родитель.\n\n" +
            "<i>Если вы передумали, нажмите /cancel</i>",
            ct: ct);

        return new StepResult<GainAccessByTokenCommand>(UserStateType.GainAccessConfirming, data);
    }
}