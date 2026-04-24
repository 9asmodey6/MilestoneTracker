namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.State;

public class ChildStepHandler(
    ITelegramMessageService messageService,
    IMilestonePreviewService previewService,
    IParentRepository parentRepository,
    ILogger<ChildStepHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneSelectingChild;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing childId for chat {ChatId}, preparing for category entering",
            context.ChatId);

        if (!int.TryParse(context.CallbackData, out var selectedChildId))
        {
            logger.LogWarning("Invalid callback data for child selection: {Data}", context.CallbackData);
            return new StepResult<CreateMilestoneData>(UserStateType.Idle, null);
        }

        var child = await parentRepository.GetChildrenByIdAsync(selectedChildId, ct);
        if (child == null)
        {
            logger.LogWarning("Child with ID {Id} not found", selectedChildId);
            throw new InvalidOperationException("Child not found");
        }

        var updatedData = data with { ChildId = child.Id, ChildName = child.Name, ChatId = context.ChatId };

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
}