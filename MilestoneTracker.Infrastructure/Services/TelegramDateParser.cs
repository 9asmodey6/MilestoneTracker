namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Shared.Interfaces;
using Microsoft.Extensions.Logging;

public class TelegramDateParser : ITelegramDateParser
{
    private readonly string[] _formats =
    {
        "dd.MM.yyyy", "d.M.yyyy",
        "dd/MM/yyyy", "d/M/yyyy",
        "dd.MM.yy", "d.M.yy"
    };

    public DateTime? ParseDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var cleanedText = text.Trim();

        if (DateTime.TryParseExact(cleanedText, _formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var result))
        {
            return DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
        }

        return null;
    }
}