namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.State;

public class TitleStepHandler(
    ITelegramMessageService messageService,
    IMilestonePreviewService previewService,
    ILogger<TitleStepHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneEnteringTitle;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing title for chat {ChatId}, preparing for description entering",
            context.ChatId);

        bool isSkipped = context.IsCallback
                         && context.CallbackData == UiConstants.CallbackQueries.Skip;

        string title;
        string confirmationText;

        if (isSkipped)
        {
            title = $"Событие от {data.Date:dd.MM.yyyy}";
            confirmationText = "Хорошо, оставим заголовок по умолчанию. 👌";
        }
        else
        {
            title = context.Text!;
            confirmationText = $"Заголовок «<b>{title}</b>» сохранен! ✅";
        }

        var updatedData = data with { Title = title };

        if (data.IsEditing)
        {
            await previewService.SendPreviewAsync(context.ChatId, updatedData, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneConfirming, updatedData);
        }
        
        await messageService.SendTextMessageAsync(
            context.ChatId,
            $"{confirmationText}\n\n" +
            "Теперь самое важное — <b>опишите, что произошло</b>. 📝\n\n" +
            "Напишите подробности: какими были первые эмоции, что именно сказал или сделал ребенок, как это случилось.",
            ct: ct);
        
        return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneEnteringDescription, updatedData);
    }
}