namespace MilestoneTracker.Application.Common.Features.Children.DeleteChild.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Models;
using Shared.Models;
using Shared.State;

public class UndoDeleteChildStepHandler : IStepHandler<DeleteChildData>
{
    public UserStateType Step => UserStateType.DeleteChildWaitingUndo;
    public Task<StepResult<DeleteChildData>> HandleAsync(BotContext context, DeleteChildData data, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}