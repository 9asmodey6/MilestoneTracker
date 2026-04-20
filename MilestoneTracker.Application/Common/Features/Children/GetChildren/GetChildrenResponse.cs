namespace MilestoneTracker.Application.Common.Features.Children.GetChildren;

public record GetChildrenResponse(
    int Id, 
    string Name, 
    string Age,
    string? PhotoFileId);