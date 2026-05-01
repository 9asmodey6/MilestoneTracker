namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone;

using System.Text;
using Application.Common.Interfaces;
using Domain.Entities.Milestones;
using Domain.Enums;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.Services;
using Shared.Models;

public class MilestoneViewService(
    ITelegramMessageService messageService,
    IMilestoneRepository milestoneRepository) : IMilestoneViewService
{
    public async Task SendMilestoneCardAsync(long chatId, Milestone milestone, string? childName,
        CancellationToken ct)
    {
        var summary = BuildSummary(milestone, childName);
        var keyboard = BotKeyboards.ViewMilestoneItemKeyboard(milestone.Id);

        if (milestone.MediaFiles.Count == 0)
        {
            await messageService.SendMessageWithInlineKeyboardAsync(chatId, summary, keyboard, ct);
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
                chatId, "⬆️ Воспоминание выше. Выберите действие:", keyboard, ct);
            return;
        }
        
        var mediaItems = milestone.MediaFiles
            .Select((m, i) => new MediaItem(m.FileId, m.Type, i == 0 ? summary : null))
            .ToList();

        await messageService.SendMediaGroupAsync(chatId, mediaItems, ct);
        await messageService.SendMessageWithInlineKeyboardAsync(
            chatId, "⬆️ Воспоминание выше. Выберите действие:", keyboard, ct);
    }

    public async Task SendMilestoneListAsync(long chatId, GetMilestoneData data, CancellationToken ct)
    {
        var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
            childId: data.ChildId!.Value,
            pageNumber: data.CurrentPage,
            category: data.Mode == ViewMode.Category ? data.SelectedCategory : null,
            specificDate: data.Mode == ViewMode.Date ? data.SelectedDate : null,
            ct: ct);

        var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

        await messageService.SendMessageWithInlineKeyboardAsync(
            chatId,
            MilestoneListMessageBuilder.BuildListMessage(data, items, data.CurrentPage, totalPages),
            BotKeyboards.PaginationKeyboard(data.CurrentPage, totalPages, items),
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
