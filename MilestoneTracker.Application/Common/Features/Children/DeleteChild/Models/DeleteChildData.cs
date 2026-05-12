namespace MilestoneTracker.Application.Common.Features.Children.DeleteChild.Models;

using Constants;
using GetChildren;

public record DeleteChildData(
    int? ChildId,
    string ChildName);