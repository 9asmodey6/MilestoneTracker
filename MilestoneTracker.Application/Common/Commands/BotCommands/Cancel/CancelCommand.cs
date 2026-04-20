namespace MilestoneTracker.Application.Common.Commands.BotCommands.Cancel;

using MediatR;

public record CancelCommand(
    long ChatId) : IRequest<Unit>;