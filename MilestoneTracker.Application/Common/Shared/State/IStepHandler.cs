namespace MilestoneTracker.Application.Common.Shared.State;

using MilestoneTracker.Application.Common.Shared.Models;
using MilestoneTracker.Domain.Enums;
using MilestoneTracker.Infrastructure.Models;

public interface IStepHandler<T>
{
    UserStateType Step { get; }
    
    Task<StepResult<T>> HandleAsync(BotContext context, T data, CancellationToken ct);
}
