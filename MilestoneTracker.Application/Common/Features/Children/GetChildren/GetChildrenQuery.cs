namespace MilestoneTracker.Application.Common.Features.Children.GetChildren;

using MediatR;

public record GetChildrenQuery(
    long ChatId) : IRequest<List<GetChildrenResponse>>;