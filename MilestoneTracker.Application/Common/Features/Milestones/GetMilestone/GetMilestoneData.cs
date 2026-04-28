namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone;

using Domain.Enums;

public enum ViewMode
{
    None,
    Latest,
    Category,
    Date
}

public record GetMilestoneData(
    int? ChildId = null,
    string? ChildName = null,
    ViewMode Mode = ViewMode.None,
    MilestoneCategory? SelectedCategory = null,
    DateTime? SelectedDate = null,
    int CurrentPage = 1,
    int? SelectedMilestoneId = null);