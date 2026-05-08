namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using Interfaces;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using MilestoneTracker.Domain.Enums;

public class MilestonePreviewService(ITelegramMessageService messageService) : IMilestonePreviewService
{
    public async Task SendPreviewAsync(long chatId, CreateMilestoneData data, CancellationToken ct)
    {
        var summary = data.GetSummary(data.ChildName);

        if (data.MediaCount == 0)
        {
            await messageService.SendMessageWithInlineKeyboardAsync(
                chatId: chatId,
                summary + "\n\nХочешь что то изменить? Если нет - нажми кнопку 'Сохранить'.",
                BotKeyboards.MilestoneConfirmationKeyboard(),
                ct);
            return;
        }

        if (data.MediaCount == 1)
        {
            var media = data.MediaGroup![0];
            if (media.Type == MediaType.Photo)
            {
                await messageService.SendPhotoAsync(
                    chatId,
                    media.FileId,
                    summary,
                   ct: ct);
            }
            else if (media.Type == MediaType.Video)
            {
                await messageService.SendVideoAsync(
                    chatId,
                    media.FileId,
                    summary,
                   ct: ct);
            }

            await messageService.SendMessageWithInlineKeyboardAsync(
                chatId: chatId,
                "Хочешь что то изменить? Если нет - нажми кнопку 'Сохранить'.",
                BotKeyboards.MilestoneConfirmationKeyboard(),
                ct);
            return;
        }

        if (data.MediaCount > 1)
        {
            var updatedData = data.AddCaption(summary);
            await messageService.SendMediaGroupAsync(
                chatId,
                updatedData.MediaGroup!,
                ct);

            await messageService.SendMessageWithInlineKeyboardAsync(
                chatId: chatId,
                "Хочешь что то изменить? Если нет - нажми кнопку 'Сохранить'.",
                BotKeyboards.MilestoneConfirmationKeyboard(),
                ct);
        }
    }
}