namespace MilestoneTracker.Application.Common.Features.Milestones.DeleteMilestone;

using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Shared.Interfaces.Services;
using Shared.State;
using Microsoft.Extensions.Logging;
using Models;

public class ProcessDeleteMilestoneStepHandler(
    ITelegramMessageService messageService,
    IEnumerable<IStepHandler<DeleteMilestoneData>> stepHandlers,
    IUserStateService userStateService,
    ILogger<ProcessDeleteMilestoneStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState == UserStateType.DeleteMilestoneConfirming
        || userState == UserStateType.DeleteMilestoneWaitingUndo;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = JsonSerializer.Deserialize<DeleteMilestoneData>(userState.StateData ?? "{}");

            if (data == null)
            {
                logger.LogWarning("State data is null for DeleteMilestone flow in chat {ChatId}", context.ChatId);
                return;
            }

            var handler = stepHandlers.FirstOrDefault(h => h.Step == userState.State);

            if (handler == null)
            {
                logger.LogWarning("No handler found for state {State} in DeleteMilestone flow", userState.State);
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
            logger.LogError(ex, "Error processing delete milestone step for ChatId: {ChatId}", context.ChatId);
            await messageService.SendTextMessageAsync(context.ChatId,
                "❌ Произошла ошибка в процессе удаления.", ct: ct);
            await userStateService.ResetAsync(context.ChatId, ct);
        }
    }
}