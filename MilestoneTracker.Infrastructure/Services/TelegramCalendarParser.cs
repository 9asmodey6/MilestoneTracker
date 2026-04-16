namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Shared.Interfaces;
using Microsoft.Extensions.Logging;

public class TelegramCalendarParser(
    ILogger<TelegramCalendarParser> logger) : ITelegramCalendarParser
{
    public DateTime? ParseDate(string callbackData)
    {
        try
        {
            if (string.IsNullOrEmpty(callbackData)) return null;

            var parts = callbackData.Split(':');
            var datePart = parts.Last();

            return DateTime.TryParse(datePart, out var result) ? result : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while parsing the callback data: {Data}", callbackData);
            return null;
        }
    }
}