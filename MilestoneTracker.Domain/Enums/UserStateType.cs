namespace MilestoneTracker.Domain.Enums;

public enum UserStateType
{
    Idle = 0,
    
    
    AddChildStarted = 10,
    AddChildEnteringName = 11,
    AddChildEnteringBirthdate = 12,
    AddChildUploadingPhoto = 13,
    
    
    AddMilestoneSelectingChild = 20,
    AddMilestoneSelectingCategory = 21,
    AddMilestoneEnteringDate = 22,
    AddMilestoneEnteringTitle = 23,
    AddMilestoneEnteringDescription = 24,
    AddMilestoneUploadingMedia = 25,
    AddMilestoneConfirming = 26,
    
}