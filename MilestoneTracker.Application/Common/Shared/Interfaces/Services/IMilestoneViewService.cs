namespace MilestoneTracker.Application.Common.Shared.Interfaces.Services;

using Domain.Entities.Milestones;
using MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Models;

using Telegram.Bot.Types.ReplyMarkups;

public interface IMilestoneViewService
{
    Task SendMilestoneCardAsync(long chatId, Milestone milestone, string? childName, string actionMessage, InlineKeyboardMarkup keyboard, CancellationToken ct);
    Task SendMilestoneListAsync(long chatId, string messageText, InlineKeyboardMarkup keyboard, CancellationToken ct);
}
