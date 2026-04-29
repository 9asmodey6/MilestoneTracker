namespace MilestoneTracker.Application.Common.Features.Children.GainAccess;

using MediatR;
using Shared.Models;

public record GainAccessByTokenCommand(
    long ParentChatId,
    string? Token) : IRequest;
