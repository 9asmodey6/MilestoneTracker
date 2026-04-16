namespace MilestoneTracker.Application.Common.Shared.Interfaces;

public interface ITelegramCalendarParser
{
    DateTime? ParseDate(string callbackData);
}