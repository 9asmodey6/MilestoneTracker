namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

using System.Globalization;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Shared.Models;
using Shared.State;

public class DateStepGetHandler(
    IMilestoneRepository milestoneRepository,
    ITelegramMessageService messageService,
    ILogger<DateStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{ 
    public UserStateType Step => UserStateType.GetMilestoneSelectingDate;

    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing date selection step for chat {ChatId}.", context.ChatId);

        if (!DateTime.TryParseExact(
                context.Text, 
                "dd.MM.yyyy", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var parsedDate))
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "⚠️ <b>Упс! Не удалось распознать дату.</b>\n\n" +
                "Пожалуйста, пришлите дату как <code>22.04.2026</code> или напишите /cancel для отмены.",
                ct: ct);
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingDate, data);
        }
        
        var utcDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        
        var updatedData = data with { SelectedDate = utcDate, CurrentPage = 1 };

        logger.LogDebug("Date {ParsedDate} selected for chat {ChatId}, loading page 1", parsedDate, context.ChatId);
        
        var (items, totalCount) = await milestoneRepository.GetPaginatedAsync(
            childId: updatedData.ChildId!.Value,
            pageNumber: updatedData.CurrentPage,
            specificDate: updatedData.SelectedDate,
            ct: ct);

        if (totalCount == 0)
        {
            logger.LogDebug("No milestones found for date {ParsedDate} for chat {ChatId}",
                parsedDate, context.ChatId);

            await messageService.SendTextMessageAsync(
                context.ChatId,
                $"📆 <b>За {parsedDate:dd.MM.yyyy} воспоминаний не найдено.</b>\n\n" +
                $"Попробуйте другую дату в формате <b>ДД.ММ.ГГГГ</b> или напишите /cancel для отмены.",
                ct: ct);

            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingDate, data);
        }

        var totalPages = MilestoneListMessageBuilder.CalculateTotalPages(totalCount);

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            MilestoneListMessageBuilder.BuildListMessage(updatedData, items, updatedData.CurrentPage, totalPages),
            BotKeyboards.PaginationKeyboard(updatedData.CurrentPage, totalPages, items),
            ct);

        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneList, updatedData);
    }
}
