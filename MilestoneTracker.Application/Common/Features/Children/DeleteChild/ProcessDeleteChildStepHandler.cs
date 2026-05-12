namespace MilestoneTracker.Application.Common.Features.Children.DeleteChild;

using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Milestones.DeleteMilestone;
using Milestones.DeleteMilestone.Models;
using Models;
using Shared.State;

public class ProcessDeleteChildStepHandler(
    ITelegramMessageService messageService,
    IEnumerable<IStepHandler<DeleteChildData>> stepHandlers,
    IUserStateService userStateService,
    ILogger<ProcessDeleteChildStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState == UserStateType.DeleteChildConfirming
        || userState == UserStateType.DeleteChildWaitingUndo;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = JsonSerializer.Deserialize<DeleteChildData>(userState.StateData ?? "{}");

            if (data == null)
            {
                logger.LogWarning("State data is null for DeleteChild flow in chat {ChatId}", context.ChatId);
                return;
            }

            var handler = stepHandlers.FirstOrDefault(h => h.Step == userState.State);

            if (handler == null)
            {
                logger.LogWarning("No handler found for state {State} in DeleteChild flow", userState.State);
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
            logger.LogError(ex, "Error processing delete child step for ChatId: {ChatId}", context.ChatId);
            await messageService.SendTextMessageAsync(context.ChatId,
                "❌ Произошла ошибка в процессе удаления.", ct: ct);
            await userStateService.ResetAsync(context.ChatId, ct);
        }
    }
}