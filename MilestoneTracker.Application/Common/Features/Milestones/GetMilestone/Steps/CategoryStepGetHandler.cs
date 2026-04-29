namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

using System;
using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Shared.Models;
using Shared.State;

public class CategoryStepGetHandler(
    IMilestoneRepository milestoneRepository,
    ITelegramMessageService messageService,
    ILogger<CategoryStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{ 
    public UserStateType Step => UserStateType.GetMilestoneSelectingCategory;

    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing category selection step for chat {ChatId}.", context.ChatId);

        if (!context.IsCallback 
            || !int.TryParse(context.CallbackData, out var categoryId)
            || !Enum.IsDefined(typeof(MilestoneCategory), categoryId))
        {
            logger.LogWarning("Invalid category callback data '{CallbackData}' for chat {ChatId}",
                context.CallbackData, context.ChatId);
            
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "🗂 Пожалуйста, выберите категорию из списка ниже или напишите /cancel для отмены:",
                BotKeyboards.CategorySelectionKeyboard(),
                ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingCategory, data);
        }

        var selectedCategory = (MilestoneCategory)categoryId;
        var updatedData = data with { SelectedCategory = selectedCategory, CurrentPage = 1 };

        logger.LogDebug("Category {Category} selected for chat {ChatId}, loading page 1", selectedCategory, context.ChatId);

        var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
            childId: updatedData.ChildId!.Value,
            pageNumber: updatedData.CurrentPage,
            category: selectedCategory,
            ct: ct);

        if (totalCount == 0)
        {
            logger.LogDebug("No milestones found in category {Category} for chat {ChatId}",
                selectedCategory, context.ChatId);

            var categoryName = MilestoneListMessageBuilder.GetCategoryName(selectedCategory);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                $"🗂 <b>В категории {categoryName} пока нет воспоминаний.</b>\n\n" +
                $"Попробуйте выбрать другую категорию или нажмите /cancel для отмены:",
                BotKeyboards.CategorySelectionKeyboard(),
                ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingCategory, updatedData);
        }

        var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            MilestoneListMessageBuilder.BuildListMessage(updatedData, items, updatedData.CurrentPage, totalPages),
            BotKeyboards.PaginationKeyboard(updatedData.CurrentPage, totalPages, items),
            ct);

        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, updatedData);
    }
}
