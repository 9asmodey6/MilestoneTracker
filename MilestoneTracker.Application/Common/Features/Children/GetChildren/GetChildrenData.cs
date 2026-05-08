namespace MilestoneTracker.Application.Common.Features.Children.GetChildren;

public record GetChildrenData(
    long? ChatId = null,
    int? ChildId = null);