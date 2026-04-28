namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.State;

public class ListStepGetHandler(
    ITelegramMessageService messageService,
    IMilestoneRepository milestoneRepository,
    IMilestoneViewService milestoneViewService,
    ILogger<ListStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{
    public UserStateType Step => UserStateType.GetMilestoneList;

    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogDebug("Processing list navigation for chat {ChatId}", context.ChatId);

        if (!context.IsCallback)
        {
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "Пожалуйста, используйте кнопки для навигации или напишите /cancel для отмены.",
                BotKeyboards.PaginationKeyboard(data.CurrentPage, 1, new()),
                ct);
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, data);
        }

        var callbackData = context.CallbackData;
        
        if (callbackData == UiConstants.CallbackQueries.GetMilestones.BackToList)
        {
            await messageService.EditMessageTextAsync(
                context.ChatId,
                context.MessageId.Value,
                "Выберите способ отображения воспоминаний:",
                BotKeyboards.ViewMilestonesModeKeyboard(),
                ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingMode, data with { CurrentPage = 1, Mode = ViewMode.None, SelectedCategory = null, SelectedDate = null });
        }
        
        if (callbackData != null && callbackData.StartsWith(UiConstants.CallbackQueries.GetMilestones.PagePrefix))
        {
            if (int.TryParse(callbackData.Replace(UiConstants.CallbackQueries.GetMilestones.PagePrefix, ""), out int targetPage))
            {
                var updatedData = data with { CurrentPage = targetPage };
                
                var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
                    childId: updatedData.ChildId!.Value,
                    pageNumber: updatedData.CurrentPage,
                    category: updatedData.Mode == ViewMode.Category ? updatedData.SelectedCategory : null,
                    specificDate: updatedData.Mode == ViewMode.Date ? updatedData.SelectedDate : null,
                    ct: ct);

                var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

                await messageService.EditMessageTextAsync(
                    context.ChatId,
                    context.MessageId.Value,
                    MilestoneListMessageBuilder.BuildListMessage(updatedData, items, updatedData.CurrentPage, totalPages),
                    BotKeyboards.PaginationKeyboard(updatedData.CurrentPage, totalPages, items),
                    ct);
                
                return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, updatedData);
            }
        }
        
        if (callbackData != null && callbackData.StartsWith(UiConstants.CallbackQueries.GetMilestones.ItemPrefix))
        {
            if (int.TryParse(callbackData.Replace(UiConstants.CallbackQueries.GetMilestones.ItemPrefix, ""), out int itemId))
            {
                var milestone = await milestoneRepository.GetByIdAsync(itemId, ct);
                if (milestone != null)
                {
                    await milestoneViewService.SendMilestoneCardAsync(context.ChatId, milestone, data.ChildName, ct);
                    
                    var updatedData = data with { SelectedMilestoneId = itemId };
                    return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneViewItem, updatedData);
                }
            }
        }

        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, data);
    }
}
