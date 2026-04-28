namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone.Steps;

using Constants;
using Domain.Entities;
using Domain.Entities.Milestones;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Models;
using Shared.State;
using Telegram.Bot.Types;

public class ConfirmingStepCreateHandler(
    ITelegramMessageService messageService,
    IMediator mediator,
    IUserStateService userStateService,
    IParentRepository parentRepository,
    ILogger<ConfirmingStepCreateHandler> logger) : IStepHandler<CreateMilestoneData>
{
    public UserStateType Step => UserStateType.AddMilestoneConfirming;

    public async Task<StepResult<CreateMilestoneData>> HandleAsync(BotContext context, CreateMilestoneData data,
        CancellationToken ct)
    {
        logger.LogDebug("Processing title for chat {ChatId}, preparing for final step",
            context.ChatId);

        if (!context.IsCallback)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "Неизвестное сообщение. Пожалуйста нажмите на кнопку ниже чтобы подтвердить создание, отменить или изменить отдельные элементы.",
                ct: ct);
            return new StepResult<CreateMilestoneData>(Step, data);
        }

        var callback = context.CallbackData;

        if (callback == UiConstants.CallbackQueries.EditMilestone.Confirm)
        {
            var command = new CreateMilestoneCommand(
                ChatId: data.ChatId.Value,
                CreatorId: data.CreatorId ?? 0,
                ChildId: data.ChildId ?? 0,
                ChildName: data.ChildName ?? string.Empty,
                Title: data.Title ?? string.Empty,
                OccuredAt: data.Date.Value,
                Description: data.Description ?? string.Empty,
                Category: data.Category ?? MilestoneCategory.General,
                MediaFiles: MapMedia(data.MediaGroup)
            );

            await mediator.Send(command, ct);
    
            await messageService.SendTextMessageAsync(context.ChatId, "✅ Воспоминание сохранено!", ct: ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<CreateMilestoneData>(UserStateType.Idle, null);
        }

        var (nextState, prompt) = callback switch
        {
            UiConstants.CallbackQueries.EditMilestone.EditChild => (UserStateType.AddMilestoneSelectingChild,
                "Выберите ребенка:"),
            UiConstants.CallbackQueries.EditMilestone.EditCategory => (UserStateType.AddMilestoneSelectingCategory,
                "Выберите новую категорию:"),
            UiConstants.CallbackQueries.EditMilestone.EditDate => (UserStateType.AddMilestoneEnteringDate,
                "Введите новую дату (ДД.ММ.ГГГГ) или сегодняшнюю с помощью кнопки ниже:"),
            UiConstants.CallbackQueries.EditMilestone.EditTitle => (UserStateType.AddMilestoneEnteringTitle,
                "Введите новый заголовок:"),
            UiConstants.CallbackQueries.EditMilestone.EditDescription => (UserStateType.AddMilestoneEnteringDescription,
                "Введите новое описание:"),
            UiConstants.CallbackQueries.EditMilestone.EditMedia => (UserStateType.AddMilestoneUploadingMedia,
                "Пришлите новые фото или видео:"),
            _ => (UserStateType.AddMilestoneConfirming, null)
        };

        var children = new List<Child>();
        if (callback == UiConstants.CallbackQueries.EditMilestone.EditChild)
        {
            children = await parentRepository.GetChildrenAsync(context.ChatId, ct);
        }

        if (prompt != null)
        {
            var updatedData = data with { IsEditing = true };

            var keyboard = callback switch
            {
                UiConstants.CallbackQueries.EditMilestone.EditCategory => BotKeyboards.CategorySelectionKeyboard(),
                UiConstants.CallbackQueries.EditMilestone.EditDate => BotKeyboards.SelectCurrentDate(),
                UiConstants.CallbackQueries.EditMilestone.EditMedia =>
                    BotKeyboards.MediaUploadKeyboard(data.MediaCount),
                UiConstants.CallbackQueries.EditMilestone.EditChild => BotKeyboards.ChildSelectionKeyboard(children),
                _ => null
            };

            if (keyboard != null)
            {
                await messageService.SendMessageWithInlineKeyboardAsync(context.ChatId, prompt, keyboard, ct);
            }
            else
            {
                await messageService.SendTextMessageAsync(context.ChatId, prompt, ct: ct);
            }

            return new StepResult<CreateMilestoneData>(nextState, updatedData);
        }

        return new StepResult<CreateMilestoneData>(Step, data);
    }
    
    private List<MilestoneMedia> MapMedia(List<MediaItem>? mediaItems)
    {
        if (mediaItems == null) return new();

        return mediaItems.Select(m => new MilestoneMedia
        {
            Id = Guid.NewGuid(),
            UploadedAt = DateTime.UtcNow,
            Type = m.Type,
            FileId = m.FileId,
            Caption = m.Caption
        }).ToList();
    }
}