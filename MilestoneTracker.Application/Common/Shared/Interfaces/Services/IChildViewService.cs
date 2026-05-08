namespace MilestoneTracker.Application.Common.Shared.Interfaces.Services;

using Domain.Entities;
using Telegram.Bot.Types.ReplyMarkups;

public interface IChildViewService
{
    Task SendChildCardAsync(long chatId, Child child, InlineKeyboardMarkup? keyboard = null, CancellationToken ct = default);
}
