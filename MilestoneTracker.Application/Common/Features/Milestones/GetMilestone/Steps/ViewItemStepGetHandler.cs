namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

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

public class ViewItemStepGetHandler(
    IMilestoneRepository milestoneRepository,
    IMilestoneViewService milestoneViewService,
    ITelegramMessageService messageService,
    ILogger<ViewItemStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{
    public UserStateType Step => UserStateType.GetMilestoneViewItem;

    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing view item step for chat {ChatId}.", context.ChatId);

        var callbackData = context.CallbackData;

        // Если это переход Назад к списку
        if (callbackData == UiConstants.CallbackQueries.GetMilestones.BackToList)
        {
            var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
                childId: data.ChildId!.Value,
                pageNumber: data.CurrentPage,
                category: data.Mode == ViewMode.Category ? data.SelectedCategory : null,
                specificDate: data.Mode == ViewMode.Date ? data.SelectedDate : null,
                ct: ct);

            var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

            // Редактируем сообщение (если это был текст) или отправляем заново
            // Т.к. карточка могла быть с медиа (фото/видео), то лучше отправить список новым сообщением 
            // или попробовать отредактировать, если это был текстовый пост.
            // Но обычно после фото/видео лучше слать новый список.
            
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                MilestoneListMessageBuilder.BuildListMessage(data, items, data.CurrentPage, totalPages),
                BotKeyboards.PaginationKeyboard(data.CurrentPage, totalPages, items),
                ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, data with { SelectedMilestoneId = null });
        }

        // Если мы только что пришли в этот стейт (например, из ListStepHandler)
        // и нам нужно показать айтем. Но в данной архитектуре мы попадаем сюда при СЛЕДУЮЩЕМ апдейте.
        // Чтобы показать айтем сразу, ListStepHandler должен сам вызвать показ.
        
        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, data);
    }
}
