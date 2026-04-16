namespace MilestoneTracker.Application.Common.Commands.State;

using Domain.Enums;
using Interfaces;
using Shared.Abstractions.Interfaces;

public class UserFlowHandlerFactory
{
    private readonly IEnumerable<IUserFlowHandler> _handlers;
    
    public UserFlowHandlerFactory(IEnumerable<IUserFlowHandler> handlers)
    {
        _handlers = handlers;
    }

    public IUserFlowHandler GetHandler(UserStateType state)
    {
        return _handlers.FirstOrDefault(h => h.CanHandle(state));
    }
}