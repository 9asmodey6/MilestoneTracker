namespace MilestoneTracker.Domain.Entities;

using Milestones;

public class Child
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string? PhotoFileId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int ParentId { get; set; } 
    public Parent Parent { get; set; } = null!; 

    public List<Milestone> Milestones { get; set; } = new();
}