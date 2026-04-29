namespace MilestoneTracker.Domain.Entities;

public class ChildAccessToken
{
    public Guid Id { get; set; }
    
    public int ChildId { get; set; }
    public Child Child { get; set; } = null!;
    
    public int CreatorId { get; set; }
    public Parent Creator { get; set; } = null!;
    
    public string Token { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    

    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public int? UsedByParentId { get; set; }
    public Parent? UsedByParent { get; set; }
    

    public int MaxUses { get; set; } = 1;
    public int CurrentUses { get; set; } = 0;
    
    public bool IsValid()
    {
        return !IsUsed 
               && DateTime.UtcNow < ExpiresAt 
               && CurrentUses < MaxUses;
    }
}