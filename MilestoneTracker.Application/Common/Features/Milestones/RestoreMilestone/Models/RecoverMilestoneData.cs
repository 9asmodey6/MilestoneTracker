namespace MilestoneTracker.Application.Common.Features.Milestones.RestoreMilestone.Models;

public record RecoverMilestoneData(
    int CurrentPage = 1,
    int? SelectedMilestoneId = null);