namespace MilestoneTracker.Application.Common.Shared.Services;

using System.Text;
using Common.Interfaces;
using Domain.Entities.Milestones;
using Domain.Enums;
using Interfaces.Services;
using Models;
using Telegram.Bot.Types.ReplyMarkups;

public class MilestoneViewService(
    ITelegramMessageService messageService) : IMilestoneViewService
{
    public async Task SendMilestoneCardAsync(long chatId, Milestone milestone, string? childName,
        string actionMessage, InlineKeyboardMarkup keyboard, CancellationToken ct)
    {
        var summary = BuildSummary(milestone, childName);

        if (milestone.MediaFiles.Count == 0)
        {
            await messageService.SendMessageWithInlineKeyboardAsync(chatId, $"{summary}\n\n{actionMessage}", keyboard, ct);
            return;
        }

        if (milestone.MediaFiles.Count == 1)
        {
            var media = milestone.MediaFiles[0];

            if (media.Type == MediaType.Photo)
                await messageService.SendPhotoAsync(chatId, media.FileId, summary, ct);
            else if (media.Type == MediaType.Video)
                await messageService.SendVideoAsync(chatId, media.FileId, summary, ct);

            await messageService.SendMessageWithInlineKeyboardAsync(
                chatId, $"⬆️ Воспоминание выше. {actionMessage}", keyboard, ct);
            return;
        }
        
        var mediaItems = milestone.MediaFiles
            .Select((m, i) => new MediaItem(m.FileId, m.Type, i == 0 ? summary : null))
            .ToList();

        await messageService.SendMediaGroupAsync(chatId, mediaItems, ct);
        await messageService.SendMessageWithInlineKeyboardAsync(
            chatId, $"⬆️ Воспоминание выше. {actionMessage}", keyboard, ct);
    }

    public async Task SendMilestoneListAsync(long chatId, string messageText, InlineKeyboardMarkup keyboard, CancellationToken ct)
    {
        await messageService.SendMessageWithInlineKeyboardAsync(
            chatId,
            messageText,
            keyboard,
            ct);
    }

    private static string BuildSummary(Milestone milestone, string? childName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"✨ <b>{milestone.Title}</b>");
        sb.AppendLine();
        sb.AppendLine($"👶 <b>Ребёнок:</b> {childName ?? "Не указан"}");
        sb.AppendLine($"📁 <b>Категория:</b> {MilestoneListMessageBuilder.GetCategoryName(milestone.Category)}");
        sb.AppendLine($"📅 <b>Дата:</b> {milestone.OccurredAt:dd.MM.yyyy}");

        if (!string.IsNullOrWhiteSpace(milestone.Description))
        {
            sb.AppendLine();
            sb.AppendLine($"📝 <b>Описание:</b>");
            sb.AppendLine(milestone.Description);
        }

        if (milestone.MediaFiles.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"🖼 <i>Медиафайлов: {milestone.MediaFiles.Count}</i>");
        }

        return sb.ToString().TrimEnd();
    }
}
