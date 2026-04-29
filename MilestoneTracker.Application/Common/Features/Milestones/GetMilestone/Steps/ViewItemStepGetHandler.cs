namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Shared.Models;
using Shared.State;

public class ViewItemStepGetHandler(
    IMilestoneRepository milestoneRepository,
    ITelegramMessageService messageService,
    IMediator mediator,
    ILogger<ViewItemStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{
    public UserStateType Step => UserStateType.GetMilestoneViewItem;

    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing view item step for chat {ChatId}, milestoneId={MilestoneId}.",
            context.ChatId, data.SelectedMilestoneId);

        // Если пришло текстовое сообщение — подсказка
        if (!context.IsCallback)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "Пожалуйста, используйте кнопки ниже для навигации или напишите /cancel для отмены.",
                ct: ct);
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneViewItem, data);
        }

        var callbackData = context.CallbackData;

        // Назад к списку
        if (callbackData == UiConstants.CallbackQueries.GetMilestones.BackToList)
        {
            var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
                childId: data.ChildId!.Value,
                pageNumber: data.CurrentPage,
                category: data.Mode == ViewMode.Category ? data.SelectedCategory : null,
                specificDate: data.Mode == ViewMode.Date ? data.SelectedDate : null,
                ct: ct);

            var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                MilestoneListMessageBuilder.BuildListMessage(data, items, data.CurrentPage, totalPages),
                BotKeyboards.PaginationKeyboard(data.CurrentPage, totalPages, items),
                ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, data with { SelectedMilestoneId = null });
        }

        // Если нажата кнопка конкретного воспоминания (например, из той же клавиатуры)
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

        // Неизвестный callback — остаёмся в текущем состоянии
        logger.LogWarning("Unknown callback '{CallbackData}' in ViewItem state for chat {ChatId}",
            callbackData, context.ChatId);

        await messageService.SendTextMessageAsync(
            context.ChatId,
            "Неизвестное действие. Используйте кнопки ниже для навигации.",
            ct: ct);

        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneViewItem, data);
    }
}
