namespace MilestoneTracker.Application.Common.Features.Children.ProvideAccess;

public record ProvideAccessData(
    int? ChildId = null,
    string? ChildName = null,
    string? GeneratedToken = null,
    DateTime? ExpiresAt = null);