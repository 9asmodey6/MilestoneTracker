namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using System.Text;
using System.Text.Json;
using Children.AddChild;
using Constants;
using Domain.Entities;
using Domain.Enums;
using Exceptions;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Interfaces;
using Telegram.Bot;

public class ProcessMilestoneStepHandler(
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ILogger<ProcessMilestoneStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState >= UserStateType.AddMilestoneStarted
        && userState <= UserStateType.AddMilestoneConfirming;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = JsonSerializer.Deserialize<CreateChildData>(userState.StateData ?? "{}")
                       ?? new CreateChildData();

            switch (userState.State)
            {
                case UserStateType.AddMilestoneStarted:
                    await HandleStartedStep(context, data, ct);
                    break;
            }
        }
        catch (ValidationException ex)
        {
            var errorMessage = new StringBuilder("⚠️ <b>Ошибки валидации:</b>\n\n");
    
            foreach (var errorGroup in ex.Errors)
            {
                foreach (var error in errorGroup.Value)
                {
                    errorMessage.AppendLine($"• {error}");
                }
            }

            errorMessage.AppendLine("\nПожалуйста, исправьте данные и попробуйте снова.");
            
            await messageService.SendTextMessageAsync(context.ChatId, errorMessage.ToString(), ct: ct);
            
            await userStateService.ResetAsync(context.ChatId, ct);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize StateData for ChatId: {ChatId}", context.ChatId);
        
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "❌ Произошла ошибка. Попробуй начать заново с кнопки '➕ Добавить воспоминание'",
                ct: ct);
        
            await userStateService.ResetAsync(context.ChatId, ct);
        }
    }

    private async Task HandleStartedStep(BotContext context, CreateChildData data, CancellationToken ct)
    {
        
    }
}