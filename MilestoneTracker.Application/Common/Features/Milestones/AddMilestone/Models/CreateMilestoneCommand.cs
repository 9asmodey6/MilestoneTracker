namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Models;

using MediatR;
using MilestoneTracker.Domain.Entities.Milestones;
using Domain.Enums;

public record CreateMilestoneCommand(
    long ChatId,
    int CreatorId,
    int ChildId,
    string ChildName,
    string Title,
    string Description,
    DateOnly OccuredAt,
    MilestoneCategory Category,
    List<MilestoneMedia>? MediaFiles) : IRequest<int>;