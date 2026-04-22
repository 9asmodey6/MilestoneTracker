namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using Domain.Enums;
using Telegram.Bot.Types;

public record CreateMilestoneData(
    int? ChildId = null,
    MilestoneCategory? Category = null,
    DateOnly? Date = null,
    string? Title = null,
    string? Description = null,
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

    public string ToString(string childName)
    {
        return $"{childName} ({ChildId})";
    }
}