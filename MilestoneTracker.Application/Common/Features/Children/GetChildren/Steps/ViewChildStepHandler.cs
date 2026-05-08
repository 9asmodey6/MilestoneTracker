namespace MilestoneTracker.Application.Common.Features.Children.GetChildren.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Shared.Models;
using Shared.State;

public class ViewChildStepHandler : IStepHandler<GetChildrenData>
{
    public UserStateType Step => UserStateType.GetChildrenViewItem;
    public Task<StepResult<GetChildrenData>> HandleAsync(BotContext context, GetChildrenData data, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}