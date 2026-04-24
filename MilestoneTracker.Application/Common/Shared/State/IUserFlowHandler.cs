namespace MilestoneTracker.Application.Common.Shared.State;

using MilestoneTracker.Domain.Entities;
using MilestoneTracker.Domain.Enums;
using MilestoneTracker.Infrastructure.Models;

public interface IUserFlowHandler
{
    bool CanHandle(UserStateType userState);
    
    Task HandleAsync(
        BotContext context,
        UserState userState,
        CancellationToken ct);
}