namespace MilestoneTracker.Domain.Enums;

public enum BotState
{
    Idle = 0,
    
    AddMilestoneSelectingChild = 10,
    AddMilestoneSelectingCategory = 11,
    AddMilestoneEnteringDate = 12,
    AddMilestoneEnteringTitle = 13,
    AddMilestoneEnteringDescription = 14,
    AddMilestoneUploadingMedia = 15,
    AddMilestoneConfirming = 16,

    AddChildEnteringName = 20,
    AddChildEnteringBirthdate = 21,
    AddChildUploadingPhoto = 22,
}