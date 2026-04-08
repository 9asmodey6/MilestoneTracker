namespace MilestoneTracker.Domain.Entities;

using Enums;

public class UserState
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public BotState State { get; set; }
    public string? StateData { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}