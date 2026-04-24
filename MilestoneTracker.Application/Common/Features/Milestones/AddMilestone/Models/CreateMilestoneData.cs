namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Models;

using MilestoneTracker.Domain.Enums;
using Telegram.Bot.Types;

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
    List<IAlbumInputMedia>? MediaGroup = null
)
{
    public CreateMilestoneData AddPhoto(string photoFileId)
    {
        List<IAlbumInputMedia> updatedList =
            [..(MediaGroup ?? []), new InputMediaPhoto(InputFile.FromFileId(photoFileId))];
        return this with { MediaGroup = updatedList };
    }

    public CreateMilestoneData AddVideo(string videoFileId)
    {
        List<IAlbumInputMedia> updatedList =
            [..(MediaGroup ?? []), new InputMediaVideo(InputFile.FromFileId(videoFileId))];
        return this with { MediaGroup = updatedList };
    }

    public CreateMilestoneData AddCaption(string caption)
    {
        if (MediaGroup == null || MediaGroup.Count == 0)
            return this;

        IAlbumInputMedia updatedFirst = MediaGroup[0] switch
        {
            InputMediaPhoto p => new InputMediaPhoto(p.Media)
            {
                Caption = caption,
                ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html
            },
            InputMediaVideo v => new InputMediaVideo(v.Media)
            {
                Caption = caption,
                ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html
            },
            _ => MediaGroup[0]
        };

        List<IAlbumInputMedia> updatedList = [updatedFirst, .. MediaGroup.Skip(1)];

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