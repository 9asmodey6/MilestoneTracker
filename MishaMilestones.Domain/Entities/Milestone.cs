namespace MishaEventTracker.Domain.Entities;

using Enums;

public class Milestone
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MilestoneCategory Category { get; set; }
    public long ParentChatId { get; set; }
}