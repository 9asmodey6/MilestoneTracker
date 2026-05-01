namespace MilestoneTracker.Application.Common.Features.Milestones.DeleteMilestone.Models;

using MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Models;

public record DeleteMilestoneData(
    int MilestoneId,
    GetMilestoneData ReturnContext);