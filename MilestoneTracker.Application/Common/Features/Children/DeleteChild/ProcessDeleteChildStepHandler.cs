namespace MilestoneTracker.Application.Common.Features.Children.DeleteChild;

using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Shared.State;

public class ProcessDeleteChildStepHandler : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState)
    {
        throw new NotImplementedException();
    }

    public Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}