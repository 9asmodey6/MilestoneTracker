namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Shared.Models;
using Shared.State;

public class ModeStepHandler(
    ITelegramMessageService messageService,
    ILogger<ModeStepHandler> logger) : IStepHandler<GetMilestoneData>
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
                "Неизвестный способ отображения восспоминаний."
                + "Пожалуйста, выберите способ из указанных ниже или напишите /cancel для отмены.",
                BotKeyboards.ViewMilestonesModeKeyboard(),
                ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingMode, data);
        }

        // No filters
        if (context.IsCallback || context.CallbackData == UiConstants.CallbackQueries.GetMilestones.ModeLatest)
        {
            updatedData = data with { Mode = ViewMode.Latest };
            
            
        }

        // Category Filter
        if (context.IsCallback || context.CallbackData == UiConstants.CallbackQueries.GetMilestones.ModeCategory)
        {
            
        }

        // Date Filter
        if (context.IsCallback || context.CallbackData == UiConstants.CallbackQueries.GetMilestones.ModeDate)
        {
            
        }
    }
}