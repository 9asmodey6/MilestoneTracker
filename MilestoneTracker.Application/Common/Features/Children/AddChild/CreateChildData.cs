namespace MilestoneTracker.Application.Common.Features.Children.AddChild;

public record CreateChildData(
    string? Name = null, 
    int? Age = null, 
    string? PhotoId = null);