namespace MilestoneTracker.Application.Common.Shared.Abstractions.Interfaces;

using MilestoneTracker.Domain.Entities;
using MilestoneTracker.Domain.Enums;
using MilestoneTracker.Infrastructure.Models;
using Telegram.Bot.Types;

public interface IUserFlowHandler
{
    bool CanHandle(UserStateType userState);
    
    Task HandleAsync(
        BotContext context,
        UserState userState,
        CancellationToken ct);
}