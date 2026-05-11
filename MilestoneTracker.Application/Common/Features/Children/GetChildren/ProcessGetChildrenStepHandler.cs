namespace MilestoneTracker.Application.Common.Features.Children.GetChildren;

using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Milestones.DeleteMilestone;
using Milestones.DeleteMilestone.Models;
using Shared.State;

public class ProcessGetChildrenStepHandler(
    ITelegramMessageService messageService,
    IEnumerable<IStepHandler<GetChildrenData>> stepHandlers,
    IUserStateService userStateService,
    ILogger<ProcessGetChildrenStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState == UserStateType.GetChildrenSelecting
        || userState == UserStateType.GetChildrenViewItem;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = JsonSerializer.Deserialize<GetChildrenData>(userState.StateData ?? "{}");

            if (data == null)
            {
                logger.LogWarning("State data is null for GetChildren flow in chat {ChatId}", context.ChatId);
                return;
            }

            var handler = stepHandlers.FirstOrDefault(h => h.Step == userState.State);

            if (handler == null)
            {
                logger.LogWarning("No handler found for state {State} in GetChildren flow", userState.State);
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
            logger.LogError(ex, "Error processing Get Children step for ChatId: {ChatId}", context.ChatId);
            await messageService.SendTextMessageAsync(context.ChatId,
                "❌ Произошла ошибка при получении списка детей.", ct: ct);
            await userStateService.ResetAsync(context.ChatId, ct);
        }
    }
}