namespace MilestoneTracker.Application.Common.Shared.Interfaces.Services;

using MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Models;

public interface IMilestonePreviewService
{
    Task SendPreviewAsync(long chatId, CreateMilestoneData data, CancellationToken ct);
}