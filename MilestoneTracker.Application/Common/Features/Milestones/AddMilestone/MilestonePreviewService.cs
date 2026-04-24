namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using Interfaces;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using Telegram.Bot.Types;

public class MilestonePreviewService(ITelegramMessageService messageService) : IMilestonePreviewService
{
    public async Task SendPreviewAsync(long chatId, CreateMilestoneData data, CancellationToken ct)
    {
        var summary = data.GetSummary(data.ChildName);

        if (data.MediaCount == 0)
        {
            await messageService.SendMessageWithInlineKeyboardAsync(
                chatId: chatId,
                "Хочешь что то изменить? Если нет - нажми кнопку 'Сохранить'." + summary,
                BotKeyboards.MilestoneConfirmationKeyboard(),
                ct);
        }

        if (data.MediaCount == 1)
        {
            var media = data.MediaGroup![0];
            if (media is InputMediaPhoto photo)
            {
                await messageService.SendPhotoAsync(
                    chatId,
                    ((InputFileId)photo.Media).Id,
                    summary,
                    ct);
                await messageService.SendMessageWithInlineKeyboardAsync(
                    chatId: chatId,
                    "Хочешь что то изменить? Если нет - нажми кнопку 'Сохранить'.",
                    BotKeyboards.MilestoneConfirmationKeyboard(),
                    ct);
                return;
            }
            else if (media is InputMediaVideo video)
            {
                await messageService.SendPhotoAsync(
                    chatId,
                    ((InputFileId)video.Media).Id,
                    summary,
                    ct);
                await messageService.SendMessageWithInlineKeyboardAsync(
                    chatId: chatId,
                    "Хочешь что то изменить? Если нет - нажми кнопку 'Сохранить'.",
                    BotKeyboards.MilestoneConfirmationKeyboard(),
                    ct);
                return;
            }

            data.AddCaption(summary);
            await messageService.SendMediaGroupAsync(
                chatId,
                data.MediaGroup,
                ct);

            await messageService.SendMessageWithInlineKeyboardAsync(
                chatId: chatId,
                "Хочешь что то изменить? Если нет - нажми кнопку 'Сохранить'.",
                BotKeyboards.MilestoneConfirmationKeyboard(),
                ct);
        }
    }
}