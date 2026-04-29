namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone;

using System.Text.Json;
using AddMilestone.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.State;

public class ProcessGetMilestoneStepHandler(
    ITelegramMessageService messageService,
    IEnumerable<IStepHandler<GetMilestoneData>> stepHandlers,
    IUserStateService userStateService,
    ILogger<ProcessGetMilestoneStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState >= UserStateType.GetMilestoneSelectingChild
        && userState <= UserStateType.GetMilestoneViewItem;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = JsonSerializer.Deserialize<GetMilestoneData>(userState.StateData ?? "{}")
                       ?? new GetMilestoneData();

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
            logger.LogError(ex, "Error processing get milestone step for ChatId: {ChatId}", context.ChatId);
            await messageService.SendTextMessageAsync(context.ChatId,
                "❌ Произошла ошибка. Попробуйте начать заново через /cancel", ct: ct);
            await userStateService.ResetAsync(context.ChatId, ct);
        }
    }
}