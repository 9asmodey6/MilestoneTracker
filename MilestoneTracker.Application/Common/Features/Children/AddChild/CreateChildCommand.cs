namespace MilestoneTracker.Application.Common.Features.Children.AddChild;
using MediatR;

public record CreateChildCommand(
    int ParentId,
    string Name,
    DateTime Date,
    string? PhotoId) : IRequest<int>;