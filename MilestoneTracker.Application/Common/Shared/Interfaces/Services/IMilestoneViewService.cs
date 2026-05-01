namespace MilestoneTracker.Application.Common.Shared.Interfaces.Services;

using MilestoneTracker.Domain.Entities.Milestones;
using MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Models;

public interface IMilestoneViewService
{
    Task SendMilestoneCardAsync(long chatId, Milestone milestone, string? childName, CancellationToken ct);
    Task SendMilestoneListAsync(long chatId, GetMilestoneData data, CancellationToken ct);
}
