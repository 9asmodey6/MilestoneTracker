namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Models;

using MilestoneTracker.Domain.Enums;
using MilestoneTracker.Application.Common.Shared.Models;

public record CreateMilestoneData(
    long? ChatId = null,
    int? CreatorId = null,
    int? ChildId = null,
    string? ChildName = null,
    MilestoneCategory? Category = null,
    DateOnly? Date = null,
    string? Title = null,
    string? Description = null,
    bool IsEditing = false,
    List<MediaItem>? MediaGroup = null
)
{
    public CreateMilestoneData AddPhoto(string photoFileId)
    {
        List<MediaItem> updatedList =
            [..(MediaGroup ?? []), new MediaItem(photoFileId, MediaType.Photo)];
        return this with { MediaGroup = updatedList };
    }

    public CreateMilestoneData AddVideo(string videoFileId)
    {
        List<MediaItem> updatedList =
            [..(MediaGroup ?? []), new MediaItem(videoFileId, MediaType.Video)];
        return this with { MediaGroup = updatedList };
    }

    public CreateMilestoneData AddCaption(string caption)
    {
        if (MediaGroup == null || MediaGroup.Count == 0)
            return this;

        MediaItem updatedFirst = MediaGroup[0] with { Caption = caption };

        List<MediaItem> updatedList = [updatedFirst, .. MediaGroup.Skip(1)];

        return this with { MediaGroup = updatedList };
    }

    public int MediaCount => MediaGroup?.Count ?? 0;

    public string GetSummary(string? childName = null)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("<b>📋 Предпросмотр воспоминания</b>");
        sb.AppendLine();
        sb.AppendLine($"<b>👶 Ребенок:</b> {childName ?? "Не выбран"}");
        sb.AppendLine($"<b>📁 Категория:</b> {Category?.ToString() ?? "<i>Не указана</i>"}");
        sb.AppendLine($"<b>📅 Дата:</b> {Date?.ToString("dd.MM.yyyy") ?? "<i>Не указана</i>"}");
        sb.AppendLine($"<b>📌 Заголовок:</b> {Title ?? "<i>Без названия</i>"}");

        if (!string.IsNullOrWhiteSpace(Description))
        {
            sb.AppendLine($"<b>📝 Описание:</b> {Description}");
        }

        sb.AppendLine();
        
        return sb.ToString();
    }
}