namespace MilestoneTracker.Application.Common.Commands.BotCommands.Help;

using MediatR;

public record HelpCommand(long ChatId) : IRequest<Unit>;
