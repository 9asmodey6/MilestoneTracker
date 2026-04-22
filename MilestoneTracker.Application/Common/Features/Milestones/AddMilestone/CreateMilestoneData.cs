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
        List<IAlbumInputMedia> updatedList = [..(MediaGroup ?? []), new InputMediaPhoto(InputFile.FromFileId(photoFileId))];
        return this with { MediaGroup = updatedList };
    }
    
    public CreateMilestoneData AddVideo(string videoFileId)
    {
        List<IAlbumInputMedia> updatedList = [..(MediaGroup ?? []), new InputMediaVideo(InputFile.FromFileId(videoFileId))];
        return this with { MediaGroup = updatedList };
    }
    
    public int MediaCount => MediaGroup?.Count ?? 0;

    public string ToString(string childName)
    {
        return $"{childName} ({ChildId})";
    }
}