namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

using Constants;
using DeleteMilestone;
using DeleteMilestone.Models;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.State;

public class ViewItemStepGetHandler(
    IMilestoneViewService viewService,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    Shared.Interfaces.Repositories.IMilestoneRepository milestoneRepository,
    IMediator mediator,
    ILogger<ViewItemStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{
    public UserStateType Step => UserStateType.GetMilestoneViewItem;

    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing view item step for chat {ChatId}, milestoneId={MilestoneId}.",
            context.ChatId, data.SelectedMilestoneId);
        
        if (!context.IsCallback)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "Пожалуйста, используйте кнопки ниже для навигации или напишите /cancel для отмены.",
                ct: ct);
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneViewItem, data);
        }

        var callbackData = context.CallbackData;

        // Back to list
        if (callbackData == UiConstants.CallbackQueries.GetMilestones.BackToList)
        {
            var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
                childId: data.ChildId!.Value,
                pageNumber: data.CurrentPage,
                category: data.Mode == ViewMode.Category ? data.SelectedCategory : null,
                specificDate: data.Mode == ViewMode.Date ? data.SelectedDate : null,
                ct: ct);

            var totalPages = Shared.Services.MilestoneListMessageBuilder.CalculateTotalPages(totalCount);
            
            var text = Shared.Services.MilestoneListMessageBuilder.BuildListMessage(data, items, data.CurrentPage, totalPages);
            var keyboard = BotKeyboards.PaginationKeyboard(
                data.CurrentPage, 
                totalPages, 
                items,
                UiConstants.CallbackQueries.GetMilestones.ItemPrefix,
                UiConstants.CallbackQueries.GetMilestones.PagePrefix,
                UiConstants.CallbackQueries.GetMilestones.BackToList);

            await viewService.SendMilestoneListAsync(context.ChatId, text, keyboard, ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, data with { SelectedMilestoneId = null });
        }
        
        if (callbackData != null && callbackData.StartsWith(UiConstants.CallbackQueries.GetMilestones.ItemPrefix))
        {
            if (int.TryParse(callbackData.Replace(UiConstants.CallbackQueries.GetMilestones.ItemPrefix, ""), out int itemId))
            {
                var query = new GetMilestoneByIdQuery(
                    ChatId: context.ChatId,
                    MilestoneId: itemId,
                    ChildName: data.ChildName);

                await mediator.Send(query, ct);
                
                return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneViewItem, data with { SelectedMilestoneId = itemId });
            }
        }

        // Delete milestone command
        if (callbackData != null && callbackData.StartsWith(UiConstants.CallbackQueries.DeleteMilestone.DeleteMilestoneCommand))
        {
            if (int.TryParse(callbackData.Replace(UiConstants.CallbackQueries.DeleteMilestone.DeleteMilestoneCommand, ""), out int itemId))
            {
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    "Вы <b>уверены</b> что хотите удалить это воспоминание?\n\nПосле удаления его можно будет восстановить в главном меню.",
                    BotKeyboards.MilestoneDeleteConfirmationKeyboard(itemId),
                    ct);

                await userStateService.UpdateAsync(
                    context.ChatId,
                    UserStateType.DeleteMilestoneConfirming,
                    new DeleteMilestoneData(
                        data.SelectedMilestoneId!.Value,
                        data), ct);
                
                return new StepResult<GetMilestoneData>(UserStateType.Idle, null);
            }
        }
        
        // Unknown callback
        logger.LogWarning("Unknown callback '{CallbackData}' in ViewItem state for chat {ChatId}",
            callbackData, context.ChatId);

        await messageService.SendTextMessageAsync(
            context.ChatId,
            "Неизвестное действие. Используйте кнопки ниже для навигации.",
            ct: ct);

        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneViewItem, data);
    }
}
