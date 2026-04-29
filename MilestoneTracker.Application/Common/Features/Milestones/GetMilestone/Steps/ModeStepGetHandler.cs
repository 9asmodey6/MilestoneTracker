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

public class ModeStepGetHandler(
    ITelegramMessageService messageService,
    IMilestoneRepository milestoneRepository,
    ILogger<ModeStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{ 
    public UserStateType Step => UserStateType.GetMilestoneSelectingMode;
    
    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogDebug("Processing mode for chat {ChatId}, preparing for list",
            context.ChatId);

        var updatedData = data;
        
        if (!context.IsCallback)
        {
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId, 
                "Неизвестный способ отображения воспоминаний. "
                + "Пожалуйста, выберите способ из указанных ниже или напишите /cancel для отмены.",
                BotKeyboards.ViewMilestonesModeKeyboard(),
                ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingMode, data);
        }

        // No filters (Latest)
        if (context.CallbackData == UiConstants.CallbackQueries.GetMilestones.ModeLatest)
        {
            updatedData = data with { Mode = ViewMode.Latest, CurrentPage = 1 };

            var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
                childId: data.ChildId!.Value,
                pageNumber: updatedData.CurrentPage,
                ct: ct);

            var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                MilestoneListMessageBuilder.BuildListMessage(updatedData, items, updatedData.CurrentPage, totalPages),
                BotKeyboards.PaginationKeyboard(updatedData.CurrentPage, totalPages, items),
                ct);
            
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, updatedData);
        }

        // Category Filter
        if (context.CallbackData == UiConstants.CallbackQueries.GetMilestones.ModeCategory)
        {
            updatedData = data with { Mode = ViewMode.Category, CurrentPage = 1 };

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "🗂 <b>Поиск по категориям</b>\n\nПожалуйста, выберите категорию воспоминаний, которую вы хотите посмотреть:",
                BotKeyboards.CategorySelectionKeyboard(),
                ct: ct);
            
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingCategory, updatedData);
        }

        // Date Filter
        if (context.CallbackData == UiConstants.CallbackQueries.GetMilestones.ModeDate)
        {
            updatedData = data with { Mode = ViewMode.Date, CurrentPage = 1 };
            
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "📆 <b>Поиск по дате</b>\n\nПожалуйста, отправьте мне дату в формате <b>ДД.ММ.ГГГГ</b> (например, <i>12.05.2023</i>), чтобы найти воспоминания за этот день.\n\n<i>(Или отправьте /cancel для отмены)</i>",
                ct: ct);
            
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingDate, updatedData);
        }
        
        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, data);
    }
}