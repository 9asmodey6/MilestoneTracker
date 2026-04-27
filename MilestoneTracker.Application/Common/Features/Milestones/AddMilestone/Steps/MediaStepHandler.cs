namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Steps;

using System.Text.Json;
using AsyncKeyedLock;
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
using Telegram.Bot.Types;

public class MediaStepHandler(
    AsyncKeyedLocker<long> keyedLocker,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    IMilestonePreviewService previewService,
    ILogger<MediaStepHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneUploadingMedia;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing title for chat {ChatId}, preparing for final step",
            context.ChatId);

        var latestData = data;

        if (context.IsCallback && context.CallbackData == UiConstants.CallbackQueries.Skip)
        {
            await previewService.SendPreviewAsync(context.ChatId, data, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneConfirming, data);
        }

        if (context.IsCallback && context.CallbackData == UiConstants.CallbackQueries.FinishMediaUpload)
        {
            if (data.MediaCount == 0)
            {
                await messageService.SendTextMessageAsync(context.ChatId, "❌ Загрузите хотя бы одно фото или видео",
                    ct: ct);
                return new StepResult<CreateMilestoneData>(Step, data);
            }

            var finalData = data.AddCaption(data.GetSummary(data.ChildName));
            await previewService.SendPreviewAsync(context.ChatId, finalData, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.AddMilestoneConfirming, finalData);
        }

        if (context.HasPhoto || context.HasVideo)
        {
            using (await keyedLocker.LockAsync(context.ChatId, ct))
            {
                var currentState = await userStateService.GetAsync(context.ChatId, ct);
                latestData = JsonSerializer.Deserialize<CreateMilestoneData>(currentState?.StateData ?? "{}") ??
                             data;
                latestData = context.HasPhoto
                    ? latestData.AddPhoto(context.PhotoFileId!)
                    : latestData.AddVideo(context.VideoFileId!);

                await userStateService.UpdateAsync(context.ChatId, Step, latestData, ct);
            }

            if (!string.IsNullOrEmpty(context.MediaGroupId))
            {
                await Task.Delay(2000, ct);
                var checkState = await userStateService.GetAsync(context.ChatId, ct);
                var checkData = JsonSerializer.Deserialize<CreateMilestoneData>(checkState?.StateData ?? "{}");
                if (checkData?.MediaCount > latestData.MediaCount)
                    return new StepResult<CreateMilestoneData>(Step, null);
            }

            await messageService.SendMessageWithInlineKeyboardAsync(context.ChatId,
                $"✅ {latestData.MediaCount} принято! Нажми 'Завершить', когда закончишь.",
                BotKeyboards.MediaUploadKeyboard(latestData.MediaCount), ct);
        }

        return new StepResult<CreateMilestoneData>(Step, null);
    }


    private static string GetMediaWord(int count)
    {
        return count switch
        {
            1 => "файл",
            2 or 3 or 4 => "файла",
            _ => "файлов"
        };
    }
}