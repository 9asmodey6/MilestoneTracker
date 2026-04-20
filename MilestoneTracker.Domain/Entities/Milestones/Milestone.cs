namespace MilestoneTracker.Domain.Entities.Milestones;

using Enums;
using ValueObjects;

public class Milestone
{
    public int Id { get; set; }
    
    public int ChildId { get; set; }
    public Child Child { get; set; } = null!;
    
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public MilestoneCategory Category { get; set; }
    public int CreatorId { get; set; } 
    public Parent Creator { get; set; } = null!;
    public List<MilestoneMedia> MediaFiles { get; set; } = new();


    public AgeInfo GetAgeAtMilestone()
    {
        return AgeInfo.Calculate(Child.BirthDate, OccurredAt);
    }
}