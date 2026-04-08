namespace MilestoneTracker.Domain.Entities.Milestones;

using Enums;

public class MilestoneMedia
{
    public Guid Id { get; set; }
    public int MilestoneId { get; set; }
    public string FileId { get; set; } = null!;
    public MediaType Type { get; set; }
    public string? Caption { get; set; }
    public DateTime UploadedAt { get; set; }
}