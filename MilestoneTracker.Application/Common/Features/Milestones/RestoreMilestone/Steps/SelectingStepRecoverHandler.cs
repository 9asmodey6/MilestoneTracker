namespace MilestoneTracker.Application.Common.Features.Milestones.RestoreMilestone.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.Services;
using Shared.State;

public class SelectingStepRecoverHandler(
    ITelegramMessageService messageService,
    IMilestoneRepository milestoneRepository,
    IUserStateService userStateService,
    IServiceProvider serviceProvider,
    ILogger<SelectingStepRecoverHandler> logger) : IStepHandler<RecoverMilestoneData>
{
    public UserStateType Step => UserStateType.RecoverMilestoneSelecting;

    public async Task<StepResult<RecoverMilestoneData>> HandleAsync(BotContext context, RecoverMilestoneData data,
        CancellationToken ct)
    {
        var updatedData = data;

        // next/prev page
        if (context.IsCallback &&
            context.CallbackData!.StartsWith(UiConstants.CallbackQueries.RecoverMilestone.PagePrefix))
        {
            var pageStr = context.CallbackData!.Replace(UiConstants.CallbackQueries.RecoverMilestone.PagePrefix, "");
            if (int.TryParse(pageStr, out var page))
            {
                updatedData = data with { CurrentPage = page };
            }
        }

        // next step if milestone was selected
        if (context.IsCallback &&
            context.CallbackData!.StartsWith(UiConstants.CallbackQueries.RecoverMilestone.ItemPrefix))
        {
            var idStr = context.CallbackData.Replace(UiConstants.CallbackQueries.RecoverMilestone.ItemPrefix, "");
            if (int.TryParse(idStr, out var milestoneId))
            {
                var confirmingHandler = serviceProvider
                    .GetServices<IStepHandler<RecoverMilestoneData>>()
                    .First(h => h.Step == UserStateType.RecoverMilestoneConfirming);
                
                return await confirmingHandler.HandleAsync(context,
                    data with { SelectedMilestoneId = milestoneId }, ct);
            }
        }

        var (items, totalCount) = await milestoneRepository.GetDeletedPaginatedAsync(
            context.ChatId,
            updatedData.CurrentPage,
            ct: ct);

        if (totalCount == 0)
        {
            await messageService.SendTextMessageAsync(context.ChatId, "🗑 <b>Ваша корзина пуста.</b>", ct: ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<RecoverMilestoneData>(UserStateType.Idle, null);
        }

        var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

        var messageText = MilestoneListMessageBuilder.BuildListMessage(
            header: "🗑 <b>Корзина удаленных воспоминаний</b>",
            items: items,
            currentPage: updatedData.CurrentPage,
            totalPages: totalPages,
            showChildName: true
        );

        var keyboard = BotKeyboards.PaginationKeyboard(
            updatedData.CurrentPage,
            totalPages,
            items,
            UiConstants.CallbackQueries.RecoverMilestone.ItemPrefix,
            UiConstants.CallbackQueries.RecoverMilestone.PagePrefix,
            UiConstants.CallbackQueries.ActionViewMilestones,
            "🔙 Назад к выбору");

        if (context.IsCallback)
        {
            await messageService.EditMessageTextAsync(context.ChatId, context.MessageId!.Value, messageText,
                keyboard, ct);
        }
        else
        {
            await messageService.SendMessageWithInlineKeyboardAsync(context.ChatId, messageText, keyboard, ct);
        }

        return new StepResult<RecoverMilestoneData>(UserStateType.RecoverMilestoneSelecting, updatedData);
    }
}