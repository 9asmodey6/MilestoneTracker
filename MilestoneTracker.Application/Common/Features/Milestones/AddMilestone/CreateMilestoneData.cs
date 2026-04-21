namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using Domain.Entities.Milestones;
using Domain.Enums;

public record CreateMilestoneData(
    int? ChildId,
    string? Title,
    string? Description,
    MilestoneCategory? Category,
    List<MilestoneMedia>? MediaFiles);