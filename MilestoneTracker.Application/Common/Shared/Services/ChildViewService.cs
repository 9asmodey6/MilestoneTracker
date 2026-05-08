namespace MilestoneTracker.Application.Common.Shared.Services;

using Common.Interfaces;
using Domain.Entities;
using Domain.Entities.ValueObjects;
using Interfaces.Services;
using Telegram.Bot.Types.ReplyMarkups;

public class ChildViewService(ITelegramMessageService messageService) : IChildViewService
{
    public async Task SendChildCardAsync(long chatId, Child child, InlineKeyboardMarkup? keyboard = null, CancellationToken ct = default)
    {
        var age = AgeInfo.Calculate(child.BirthDate, DateTime.UtcNow).ToString();
        
        var caption = 
            $"<b>👶 Ребенок:</b> {child.Name}\n" +
            $"<b>🎂 Возраст:</b> {age}\n" +
            $"──────────────────\n" +
            $"<i>Выберите действие ниже, чтобы просмотреть вехи или изменить данные.</i>";

        if (!string.IsNullOrEmpty(child.PhotoFileId))
        {
            await messageService.SendPhotoAsync(chatId, child.PhotoFileId, caption, keyboard, ct);
        }
        else
        {
            await messageService.SendMessageWithInlineKeyboardAsync(chatId, caption, keyboard, ct);
        }
    }
}
