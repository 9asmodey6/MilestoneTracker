namespace MilestoneTracker.Application.Common.Shared.Interfaces;

public interface ITelegramDateParser
{
    DateTime? ParseDate(string callbackData);
}