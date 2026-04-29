namespace MilestoneTracker.Application.Common.Features.Children.GainAccess;

using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using ProvideAccess;
using Shared.State;

public class ProcessGainByTokenStepHandler(
    IEnumerable<IStepHandler<GainAccessByTokenCommand>> stepHandlers,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ILogger<ProcessGainByTokenStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState >= UserStateType.GainAccessEnteringToken
        && userState <= UserStateType.GainAccessConfirming;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = new GainAccessByTokenCommand(context.ChatId, null);

            var handler = stepHandlers.FirstOrDefault(h => h.Step == userState.State);

            if (handler == null)
            {
                logger.LogWarning("No handler found for state {State}", userState.State);
                return;
            }

            var result = await handler.HandleAsync(context, data, ct);

            if (result.UpdatedData != null)
            {
                await userStateService.UpdateAsync(context.ChatId, result.NextState, result.UpdatedData, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing gaining access by token step for ChatId: {ChatId}", context.ChatId + ex.Message);
            await messageService.SendTextMessageAsync(context.ChatId,
                "❌ Произошла ошибка. Попробуйте начать заново через /cancel", ct: ct);
            await userStateService.ResetAsync(context.ChatId, ct);
        }
    }
}