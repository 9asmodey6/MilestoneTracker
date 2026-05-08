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
using Shared.State;

public class ConfirmingStepRecoverHandler(
    ITelegramMessageService messageService,
    IMilestoneViewService milestoneViewService,
    IMilestoneRepository milestoneRepository,
    IUserStateService userStateService,
    ILogger<ConfirmingStepRecoverHandler> logger) : IStepHandler<RecoverMilestoneData>
{
    public UserStateType Step => UserStateType.RecoverMilestoneConfirming;

    public async Task<StepResult<RecoverMilestoneData>> HandleAsync(BotContext context, RecoverMilestoneData data,
        CancellationToken ct)
    {
        if (context.IsCallback && context.CallbackData == UiConstants.CallbackQueries.RecoverMilestone.Confirm)
        {
            logger.LogInformation("Processing milestone {milestoneId} recovering for chat {chatId}",
                data.SelectedMilestoneId, context.ChatId);

            var isSucceeded = await milestoneRepository.RecoverAsync(data.SelectedMilestoneId!.Value, ct) > 0;

            if (!isSucceeded)
            {
                logger.LogError("Failed to recover milestone {MilestoneId} for ChatId: {ChatId}. Repository returned 0 rows affected.", 
                    data.SelectedMilestoneId, context.ChatId);
                await messageService.SendTextMessageAsync(
                    context.ChatId,
                    "❌ <b>Произошла ошибка при восстановлении</b>\n\n" +
                    "К сожалению, нам не удалось восстановить это воспоминание. Возможно, оно уже было восстановлено или удалено окончательно. " +
                    "Пожалуйста, попробуйте обновить список через <b>/cancel</b>.",
                    ct: ct);

                await userStateService.ResetAsync(context.ChatId, ct);
                return new StepResult<RecoverMilestoneData>(UserStateType.Idle, null);
            }

            await messageService.SendTextMessageAsync(
                context.ChatId,
                "✅ <b>Воспоминание успешно восстановлено!</b>\nТеперь оно снова отображается в общем списке.",
                ct: ct);

            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<RecoverMilestoneData>(UserStateType.Idle, null);
        }

        var milestone = await milestoneRepository.GetByIdWithDeletedAsync(data.SelectedMilestoneId!.Value, ct);

        if (milestone == null)
        {
            await messageService.SendTextMessageAsync(context.ChatId,
                "⚠️ Воспоминание не найдено или уже было восстановлено.", ct: ct);
           await userStateService.ResetAsync(context.ChatId, ct);
           return new StepResult<RecoverMilestoneData>(UserStateType.Idle, null);
        }
        
        await milestoneViewService.SendMilestoneCardAsync(
            context.ChatId,
            milestone,
            milestone.Child.Name,
            "Вы хотите восстановить это воспоминание?",
            BotKeyboards.MilestoneRecoveryConfirmationKeyboard(),
            ct);
        return new StepResult<RecoverMilestoneData>(UserStateType.RecoverMilestoneConfirming, data);
    }
}

