namespace MilestoneTracker.Application.Common.Features.Children.AddChild;

public record CreateChildData(
    string? Name = null, 
    DateTime? BirthDate = null, 
    string? PhotoId = null);