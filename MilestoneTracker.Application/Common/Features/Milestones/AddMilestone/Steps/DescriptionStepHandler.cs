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

public class DescriptionStepHandler(
    ITelegramMessageService messageService,
    IMilestonePreviewService previewService,
    ILogger<DescriptionStepHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneEnteringDescription;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogDebug("Processing description for chat {ChatId}, preparing for media upload",
            context.ChatId);

        if (string.IsNullOrWhiteSpace(context.Text))
        {
            logger.LogWarning("Description text is missing or empty for chat {ChatId}", context.ChatId);

            await messageService.SendTextMessageAsync(
                context.ChatId,
                "⚠️ <b>Описание не может быть пустым.</b>\n\n" +
                "Пожалуйста, напишите несколько слов о том, что произошло, чтобы сохранить это воспоминание:",
                ct: ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneEnteringDescription, data);
        }

        var updatedData = data with { Description = context.Text };

        if (data.IsEditing)
        {
            await previewService.SendPreviewAsync(context.ChatId, updatedData, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneConfirming, updatedData);
        }
        
        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "Текст принят! ✍️\n\n" +
            "Теперь вы можете <b>прислать фотографию или видео</b> к этому событию, чтобы оно стало ещё ярче.\n\n" +
            "Если не хотите добавлять медиа, просто нажмите кнопку <b>Пропустить</b> 👇",
            BotKeyboards.SkipKeyboard(),
            ct);
        return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneUploadingMedia, updatedData);
    }
}