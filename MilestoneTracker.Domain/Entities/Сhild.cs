namespace MilestoneTracker.Domain.Entities;

using Milestones;

public class Child
{
    public int Id { get; set; }
    public string Name { get; set;  }
    public DateTime BirthDate { get; set; }
    public string? PhotoFileId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Milestone> Milestones { get; set; } = new();
    public ICollection<Parent> Parents { get; set; } = null!;
}