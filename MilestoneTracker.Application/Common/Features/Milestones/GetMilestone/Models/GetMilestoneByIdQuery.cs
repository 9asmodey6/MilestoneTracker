namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Models;

using MediatR;

public record GetMilestoneByIdQuery(
    long ChatId,
    int MilestoneId,
    string? ChildName) : IRequest<Unit>;
