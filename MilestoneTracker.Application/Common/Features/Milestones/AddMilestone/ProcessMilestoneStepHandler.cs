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
using Shared.Bot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

public class ProcessMilestoneStepHandler(
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    IParentRepository parentRepository,
    ILogger<ProcessMilestoneStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState >= UserStateType.AddMilestoneStarted
        && userState <= UserStateType.AddMilestoneConfirming;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = JsonSerializer.Deserialize<CreateMilestoneData>(userState.StateData ?? "{}")
                       ?? new CreateMilestoneData();

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

    private async Task HandleStartedStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
    {
        logger.LogInformation("Started milestone adding step for chat {ChatId}, waiting for name entering",
            context.ChatId);
        
        var children = await parentRepository.GetChildrenAsync(context.ChatId, ct);
        if (children.Count == 0)
        {
            logger.LogError("Children not found for chat {ChatId}", context.ChatId);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "<b>В базе пока нет ваших детей.</b>\n\nНажмите кнопку ниже, чтобы добавить первого ребенка и начать отслеживать его достижения! 👇",
                BotKeyboards.AddChildKeyboard,
                ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return;
        }

        if (children.Count == 1)
        {
            var child = children.First();
            var updatedData = data with { ChildId = child.Id };
            
            
        }
    }
}