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

public class CategoryStepHandler(
    ITelegramMessageService messageService,
    IMilestonePreviewService previewService,
    ILogger<CategoryStepHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneSelectingCategory;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogDebug("Processing category for chat {ChatId}, preparing for date entering",
            context.ChatId);

        if (!int.TryParse(context.CallbackData, out var selectedCategoryId)
            || !Enum.IsDefined(typeof(MilestoneCategory), selectedCategoryId))
        {
            logger.LogWarning("Invalid callback data for category selection: {Data}", context.CallbackData);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "⚠️ <b>Упс! Не удалось распознать категорию.</b>\n\n" +
                "Пожалуйста, выберите один из вариантов на кнопках выше. Если что-то идет не так, просто введите <b>/cancel</b>, чтобы начать заново.",
                BotKeyboards.CategorySelectionKeyboard(),
                ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneSelectingCategory, data);
        }

        var updatedData = data with { Category = (MilestoneCategory)selectedCategoryId };

        if (data.IsEditing)
        {
            await previewService.SendPreviewAsync(context.ChatId, updatedData, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneConfirming, updatedData);
        }
        
        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "Категория выбрана! ✅\n\n" +
            "Теперь укажите <b>дату этого события</b>. Вы можете нажать на кнопку ниже, если это произошло сегодня, или прислать дату сообщением в формате <code>ДД.ММ.ГГГГ</code> (например, 22.04.2026):",
            BotKeyboards.SelectCurrentDate(),
            ct);
    
        return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneEnteringDate, updatedData);
    }
}