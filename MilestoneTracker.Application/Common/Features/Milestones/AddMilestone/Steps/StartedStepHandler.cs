namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.State;

public class StartedStepHandler(
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    IParentRepository parentRepository,
    IMilestonePreviewService previewService,
    ILogger<StartedStepHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneStarted;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Started milestone adding step for chat {ChatId}",
            context.ChatId);

        var parent = await parentRepository.GetWithChildrenAsync(context.ChatId, ct);

        if (parent == null)
        {
            logger.LogWarning("Parent with {chatId} not found in database", context.ChatId);
            return new StepResult<CreateMilestoneData>(UserStateType.Idle, null);
        }

        if (parent.Children.Count == 0)
        {
            logger.LogError("Children not found for chat {ChatId}", context.ChatId);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "<b>В базе пока нет ваших детей.</b>\n\nНажмите кнопку ниже, чтобы добавить первого ребенка и начать отслеживать его достижения! 👇",
                BotKeyboards.AddChildKeyboard,
                ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.Idle, null);
        }

        if (parent.Children.Count == 1)
        {
            var child = parent.Children.First();
            var updatedData = data with { ChildId = child.Id, CreatorId = parent.Id, ChildName = child.Name, ChatId = context.ChatId };
            ;

            if (data.IsEditing)
            {
                await previewService.SendPreviewAsync(context.ChatId, updatedData, ct);
                return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneConfirming, updatedData);
            }

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                $"Отлично! Добавляем воспоминание для <b>{child.Name}</b> ✨\n\n" +
                "Пожалуйста, <i>выберите категорию</i> из списка ниже:",
                BotKeyboards.CategorySelectionKeyboard(),
                ct);

            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneSelectingCategory, updatedData);
        }

        var dataWithCreatorId = data with {ChatId = context.ChatId, CreatorId = parent.Id};
        
        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "Для кого именно мы запишем это событие? Пожалуйста, <i>выберите ребенка</i> из списка ниже:",
            BotKeyboards.ChildSelectionKeyboard(parent.Children.ToList()),
            ct);
        
        return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneSelectingChild, dataWithCreatorId);
    }
}