namespace MilestoneTracker.Application.Common.Shared.State;

using MilestoneTracker.Domain.Enums;

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