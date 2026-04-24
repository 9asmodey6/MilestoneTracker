namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.State;

public class DateStepHandler(
    ITelegramMessageService messageService,
    IMilestonePreviewService previewService,
    ILogger<DateStepHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneEnteringDate;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing date for chat {ChatId}, preparing for title entering",
            context.ChatId);

        DateOnly parsedDate = new DateOnly();

        if (!string.IsNullOrEmpty(context.CallbackData))
        {
            if (!DateOnly.TryParseExact(context.CallbackData, "yyyy-MM-dd", out parsedDate))
            {
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    "⚠️ <b>Упс! Не удалось распознать дату.</b>\n\n" +
                    "Пожалуйста, пришлите дату как <code>22.04.2026</code> или нажмите кнопку.",
                    BotKeyboards.SelectCurrentDate(),
                    ct);
                return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneEnteringDate, data);
            }
        }

        else if (!string.IsNullOrEmpty(context.Text))
        {
            if (!DateOnly.TryParseExact(context.Text, "dd.MM.yyyy", out parsedDate))
            {
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    "⚠️ <b>Упс! Не удалось распознать дату.</b>\n\n" +
                    "Пожалуйста, пришлите дату как <code>22.04.2026</code> или нажмите кнопку.",
                    BotKeyboards.SelectCurrentDate(),
                    ct);
                return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneEnteringDate, data);
            }
        }

        var updatedData = data with { Date = parsedDate };

        if (data.IsEditing)
        {
            await previewService.SendPreviewAsync(context.ChatId, updatedData, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneConfirming, updatedData);
        }

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            $"Дата события: <b>{parsedDate:dd.MM.yyyy}</b> ✅\n\n" +
            "Принято! Теперь придумайте <b>короткий заголовок</b> для этого воспоминания.\n\n" +
            "Например: <i>«Первое слово \"Мама\"»</i>, <i>«Первые шаги»</i> или <i>«Поход в зоопарк»</i>.\n\n" +
            "Если хотите оставить заголовок по умолчанию, нажмите кнопку <b>Пропустить</b> снизу 👇\n\n" +
            "Это поможет вам позже быстро найти событие в ленте.",
            BotKeyboards.SkipKeyboard(),
            ct: ct);
        
        return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneEnteringTitle, updatedData);
    }
}