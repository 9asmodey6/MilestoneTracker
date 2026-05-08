namespace MilestoneTracker.Application.Common.Features.Milestones.DeleteMilestone.Steps;

using Microsoft.Extensions.Logging;
using MilestoneTracker.Application.Common.Constants;
using MilestoneTracker.Application.Common.Interfaces;
using MilestoneTracker.Application.Common.Shared.Interfaces.Repositories;
using MilestoneTracker.Application.Common.Shared.Interfaces.Services;
using MilestoneTracker.Application.Common.Shared.Models;
using MilestoneTracker.Application.Common.Shared.State;
using MilestoneTracker.Domain.Enums;
using MilestoneTracker.Infrastructure.Models;
using Models;
using Telegram.Bot.Types.ReplyMarkups;

public class ConfirmDeleteStepHandler(
    IMilestoneRepository milestoneRepository,
    IMilestoneViewService viewService,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ILogger<ConfirmDeleteStepHandler> logger) : IStepHandler<DeleteMilestoneData>
{
    public UserStateType Step => UserStateType.DeleteMilestoneConfirming;

    public async Task<StepResult<DeleteMilestoneData>> HandleAsync(BotContext context, DeleteMilestoneData data,
        CancellationToken ct)
    {
        if (!context.IsCallback)
        {
            await messageService.SendTextMessageAsync(context.ChatId,
                "Пожалуйста, используйте кнопки для подтверждения или отмены удаления.", ct: ct);
            return new StepResult<DeleteMilestoneData>(Step, data);
        }

        var callbackData = context.CallbackData;

        // Performing deletion
        if (callbackData != null && callbackData.StartsWith(UiConstants.CallbackQueries.DeleteMilestone.ConfirmDeletePrefix))
        {
                var result = await milestoneRepository.SoftDeleteAsync(context.ChatId, data.MilestoneId, ct);

                if (result == 0)
                {
                    await messageService.SendTextMessageAsync(
                        context.ChatId,
                        "<b>Произошла ошибка во время удаления восспоминания. Попробуйте ещё раз.</b>");
                    var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
                        childId: data.ReturnContext.ChildId!.Value,
                        pageNumber: data.ReturnContext.CurrentPage,
                        category: data.ReturnContext.Mode == GetMilestone.Models.ViewMode.Category ? data.ReturnContext.SelectedCategory : null,
                        specificDate: data.ReturnContext.Mode == GetMilestone.Models.ViewMode.Date ? data.ReturnContext.SelectedDate : null,
                        ct: ct);

                    var totalPages = Shared.Services.MilestoneListMessageBuilder.CalculateTotalPages(totalCount);
                    var text = Shared.Services.MilestoneListMessageBuilder.BuildListMessage(data.ReturnContext, items, data.ReturnContext.CurrentPage, totalPages);
                    var keyboard = Shared.Bot.Keyboards.BotKeyboards.PaginationKeyboard(
                        data.ReturnContext.CurrentPage, totalPages, items,
                        UiConstants.CallbackQueries.GetMilestones.ItemPrefix,
                        UiConstants.CallbackQueries.GetMilestones.PagePrefix,
                        UiConstants.CallbackQueries.GetMilestones.BackToList);
                        
                    await viewService.SendMilestoneListAsync(context.ChatId, text, keyboard, ct);
                    await userStateService.UpdateAsync(context.ChatId, UserStateType.GetMilestoneList, data.ReturnContext, ct);
                    return  new StepResult<DeleteMilestoneData>(UserStateType.Idle, null);
                }

                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    "🗑 Воспоминание удалено. Вы можете восстановить его сейчас, если передумали.",
                    new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("🔄 Восстановить", UiConstants.CallbackQueries.DeleteMilestone.RestoreCommand) },
                        new[] { InlineKeyboardButton.WithCallbackData("📜 К списку", UiConstants.CallbackQueries.GetMilestones.BackToList) }
                    }),
                    ct);

                return new StepResult<DeleteMilestoneData>(UserStateType.DeleteMilestoneWaitingUndo, data);
        }

        // Back to milestone list
        if (callbackData == UiConstants.CallbackQueries.GetMilestones.BackToList)
        {
            var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
                childId: data.ReturnContext.ChildId!.Value,
                pageNumber: data.ReturnContext.CurrentPage,
                category: data.ReturnContext.Mode == GetMilestone.Models.ViewMode.Category ? data.ReturnContext.SelectedCategory : null,
                specificDate: data.ReturnContext.Mode == GetMilestone.Models.ViewMode.Date ? data.ReturnContext.SelectedDate : null,
                ct: ct);

            var totalPages = Shared.Services.MilestoneListMessageBuilder.CalculateTotalPages(totalCount);
            var text = Shared.Services.MilestoneListMessageBuilder.BuildListMessage(data.ReturnContext, items, data.ReturnContext.CurrentPage, totalPages);
            var keyboard = Shared.Bot.Keyboards.BotKeyboards.PaginationKeyboard(
                data.ReturnContext.CurrentPage, totalPages, items,
                UiConstants.CallbackQueries.GetMilestones.ItemPrefix,
                UiConstants.CallbackQueries.GetMilestones.PagePrefix,
                UiConstants.CallbackQueries.GetMilestones.BackToList);

            await viewService.SendMilestoneListAsync(context.ChatId, text, keyboard, ct);
            
            await userStateService.UpdateAsync(context.ChatId, UserStateType.GetMilestoneList, data.ReturnContext, ct);
            return new StepResult<DeleteMilestoneData>(UserStateType.GetMilestoneList, null);
        }

        // Back to milestone card
        if (callbackData != null && callbackData.StartsWith(UiConstants.CallbackQueries.GetMilestones.ItemPrefix))
        {
            var milestone = await milestoneRepository.GetByIdAsync(data.MilestoneId, ct);
            if (milestone != null)
            {
                var keyboard = Shared.Bot.Keyboards.BotKeyboards.ViewMilestoneItemKeyboard(
                    milestone.Id,
                    UiConstants.CallbackQueries.GetMilestones.BackToList,
                    UiConstants.CallbackQueries.DeleteMilestone.DeleteMilestoneCommand,
                    "Удалить восспоминание",
                    "🗑️");

                await viewService.SendMilestoneCardAsync(context.ChatId, milestone, data.ReturnContext.ChildName, "Выберите действие:", keyboard, ct);
                
                await userStateService.UpdateAsync(context.ChatId, UserStateType.GetMilestoneViewItem, data.ReturnContext, ct);
                return new StepResult<DeleteMilestoneData>(UserStateType.GetMilestoneViewItem, null);
            }
        }

        return new StepResult<DeleteMilestoneData>(Step, data);
    }
}
