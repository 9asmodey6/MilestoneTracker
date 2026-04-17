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

            if (!callbackData.StartsWith("date:")) return null;

            var datePart = callbackData.Replace("date:", "");

            return DateTime.TryParseExact(
                datePart,
                "yyyy-MM-dd",
                null,
                System.Globalization.DateTimeStyles.None,
                out var result)
                ? result
                : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing date from: {Data}", callbackData);
            return null;
        }
    }
}