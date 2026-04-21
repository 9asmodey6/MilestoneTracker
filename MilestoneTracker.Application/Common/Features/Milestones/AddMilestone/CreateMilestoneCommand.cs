namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using MilestoneTracker.Domain.Entities.Milestones;
using Domain.Enums;

public record CreateMilestoneCommand(
    int CreatorId,
    int ChildId,
    string Title,
    string Description,
    MilestoneCategory Category,
    List<MilestoneMedia>? MediaFiles);