namespace MilestoneTracker.Application.Common.Features.Milestones.DeleteMilestone.Steps;

using MilestoneTracker.Application.Common.Constants;
using MilestoneTracker.Application.Common.Interfaces;
using MilestoneTracker.Application.Common.Shared.Interfaces.Repositories;
using MilestoneTracker.Application.Common.Shared.Interfaces.Services;
using MilestoneTracker.Application.Common.Shared.Models;
using MilestoneTracker.Application.Common.Shared.State;
using MilestoneTracker.Domain.Enums;
using MilestoneTracker.Infrastructure.Models;
using Models;

public class UndoDeleteStepHandler(
    IMilestoneRepository milestoneRepository,
    IMilestoneViewService viewService,
    ITelegramMessageService messageService,
    IUserStateService userStateService) : IStepHandler<DeleteMilestoneData>
{
    public UserStateType Step => UserStateType.DeleteMilestoneWaitingUndo;

    public async Task<StepResult<DeleteMilestoneData>> HandleAsync(BotContext context, DeleteMilestoneData data,
        CancellationToken ct)
    {
        if (!context.IsCallback)
        {
            return new StepResult<DeleteMilestoneData>(Step, data);
        }

        var callbackData = context.CallbackData;

        // Undo
        if (callbackData == UiConstants.CallbackQueries.DeleteMilestone.RestoreCommand)
        {
            var milestone = await milestoneRepository.GetByIdAsync(data.MilestoneId, ct);
            if (milestone != null)
            {
                milestone.IsDeleted = false;
                milestone.DeletedAt = null;
                await milestoneRepository.UpdateAsync(milestone, ct);

                await messageService.SendTextMessageAsync(context.ChatId, "✅ Воспоминание восстановлено!", ct: ct);
                
                await viewService.SendMilestoneCardAsync(context.ChatId, milestone, data.ReturnContext.ChildName, ct);
                
                await userStateService.UpdateAsync(context.ChatId, UserStateType.GetMilestoneViewItem, data.ReturnContext, ct);
                return new StepResult<DeleteMilestoneData>(UserStateType.GetMilestoneViewItem, null);
            }
        }

        // Back to list
        if (callbackData == UiConstants.CallbackQueries.GetMilestones.BackToList)
        {
            await viewService.SendMilestoneListAsync(context.ChatId, data.ReturnContext, ct);
            
            await userStateService.UpdateAsync(context.ChatId, UserStateType.GetMilestoneList, data.ReturnContext, ct);
            return new StepResult<DeleteMilestoneData>(UserStateType.GetMilestoneList, null);
        }

        return new StepResult<DeleteMilestoneData>(Step, data);
    }
}
