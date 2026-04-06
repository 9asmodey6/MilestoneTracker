namespace MilestoneTracker.Domain.Entities;

public class Parent
{
    public int Id { get; set; }
    public long ChatId { get; set; } 
    public string Name { get; set; } = null!;
    public ICollection<Child> Children { get; set; } = new List<Child>();
}