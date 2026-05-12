namespace MilestoneTracker.Domain.Enums;

public enum UserStateType
{
    Idle = 0,
    
    
    AddChildStarted = 10,
    AddChildEnteringName = 11,
    AddChildEnteringBirthdate = 12,
    AddChildUploadingPhoto = 13,
    
    
    AddMilestoneStarted = 20,
    AddMilestoneSelectingChild = 21,
    AddMilestoneSelectingCategory = 22,
    AddMilestoneEnteringDate = 23,
    AddMilestoneEnteringTitle = 24,
    AddMilestoneEnteringDescription = 25,
    AddMilestoneUploadingMedia = 26,
    AddMilestoneConfirming = 27,
    
    
    GetMilestoneSelectingChild = 30,
    GetMilestoneSelectingMode = 31,
    GetMilestoneSelectingCategory = 32,
    GetMilestoneSelectingDate = 33,
    GetMilestoneList = 34,
    GetMilestoneViewItem = 35,
    
    ProvideAccessSelectingChild = 40,
    ProvideAccessConfirming = 41,
    
    GainAccessEnteringToken = 50,
    GainAccessConfirming = 51,
    
    DeleteMilestoneConfirming = 60,
    DeleteMilestoneWaitingUndo = 61,
    
    RecoverMilestoneSelecting = 70,
    RecoverMilestoneConfirming = 71,
    
    GetChildrenSelecting = 80,
    GetChildrenViewItem = 81,
    
    DeleteChildConfirming = 90,
    DeleteChildWaitingUndo = 91,
}