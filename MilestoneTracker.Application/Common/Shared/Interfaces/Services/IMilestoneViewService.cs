namespace MilestoneTracker.Application.Common.Shared.Interfaces.Services;

using MilestoneTracker.Domain.Entities.Milestones;

public interface IMilestoneViewService
{
    /// <summary>
    /// Отправляет пользователю красивую карточку воспоминания с медиа (если есть).
    /// </summary>
    Task SendMilestoneCardAsync(long chatId, Milestone milestone, string? childName, CancellationToken ct);
}
