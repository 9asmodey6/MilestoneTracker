namespace MilestoneTracker.Application.Common.Commands.Bot.Start;

using MediatR;

public record StartCommand : IRequest<Unit>
{
    public long ChatId { get; init; }
    public string? FirstName { get; init; }
    public string? Username { get; init; }
 
    public StartCommand(long chatId, string? firstName = null, string? username = null)
    {
        ChatId = chatId;
        FirstName = firstName;
        Username = username;
    }
}