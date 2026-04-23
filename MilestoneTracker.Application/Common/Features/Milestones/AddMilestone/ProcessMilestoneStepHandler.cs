namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using System.Text;
using System.Text.Json;
using Constants;
using Domain.Entities;
using Domain.Enums;
using Exceptions;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Interfaces;
using Shared.Bot.Keyboards;
using Telegram.Bot.Types;

public class ProcessMilestoneStepHandler(
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    IParentRepository parentRepository,
    ILogger<ProcessMilestoneStepHandler> logger) : IUserFlowHandler
{
    private static readonly SemaphoreSlim StateLock = new(1, 1);

    public bool CanHandle(UserStateType userState) =>
        userState >= UserStateType.AddMilestoneStarted
        && userState <= UserStateType.AddMilestoneConfirming;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        try
        {
            var data = JsonSerializer.Deserialize<CreateMilestoneData>(userState.StateData ?? "{}")
                       ?? new CreateMilestoneData();

            switch (userState.State)
            {
                case UserStateType.AddMilestoneStarted:
                    await HandleStartedStep(context, data, ct);
                    break;
                case UserStateType.AddMilestoneSelectingChild:
                    await HandleChildStep(context, data, ct);
                    break;
                case UserStateType.AddMilestoneSelectingCategory:
                    await HandleCategoryStep(context, data, ct);
                    break;
                case UserStateType.AddMilestoneEnteringDate:
                    await HandleDateStep(context, data, ct);
                    break;
                case UserStateType.AddMilestoneEnteringTitle:
                    await HandleTitleStep(context, data, ct);
                    break;
                case UserStateType.AddMilestoneEnteringDescription:
                    await HandleDescriptionStep(context, data, ct);
                    break;
                case UserStateType.AddMilestoneUploadingMedia:
                    await HandleMediaStep(context, data, ct);
                    break;
                case UserStateType.AddMilestoneConfirming:
                    await HandleConfirmingAsync(context, data, ct);
                    break;
            }
        }
        catch (ValidationException ex)
        {
            var errorMessage = new StringBuilder("⚠️ <b>Ошибки валидации:</b>\n\n");

            foreach (var errorGroup in ex.Errors)
            {
                foreach (var error in errorGroup.Value)
                {
                    errorMessage.AppendLine($"• {error}");
                }
            }

            errorMessage.AppendLine("\nПожалуйста, исправьте данные и попробуйте снова.");

            await messageService.SendTextMessageAsync(context.ChatId, errorMessage.ToString(), ct: ct);

            await userStateService.ResetAsync(context.ChatId, ct);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize StateData for ChatId: {ChatId}", context.ChatId);

            await messageService.SendTextMessageAsync(
                context.ChatId,
                "❌ Произошла ошибка. Попробуй начать заново с кнопки '➕ Добавить воспоминание'",
                ct: ct);

            await userStateService.ResetAsync(context.ChatId, ct);
        }
    }

    private async Task HandleStartedStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
    {
        logger.LogInformation("Started milestone adding step for chat {ChatId}",
            context.ChatId);

        var children = await parentRepository.GetChildrenAsync(context.ChatId, ct);
        if (children.Count == 0)
        {
            logger.LogError("Children not found for chat {ChatId}", context.ChatId);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "<b>В базе пока нет ваших детей.</b>\n\nНажмите кнопку ниже, чтобы добавить первого ребенка и начать отслеживать его достижения! 👇",
                BotKeyboards.AddChildKeyboard,
                ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return;
        }

        if (children.Count == 1)
        {
            var child = children.First();
            var updatedData = data with { ChildId = child.Id };

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                $"Отлично! Добавляем воспоминание для <b>{child.Name}</b> ✨\n\n" +
                "Пожалуйста, <i>выберите категорию</i> из списка ниже:",
                BotKeyboards.CategorySelectionKeyboard(),
                ct);

            await userStateService.UpdateAsync(
                context.ChatId,
                UserStateType.AddMilestoneSelectingCategory,
                updatedData,
                ct);

            return;
        }

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "Для кого именно мы запишем это событие? Пожалуйста, <i>выберите ребенка</i> из списка ниже:",
            BotKeyboards.ChildSelectionKeyboard(children),
            ct);

        await userStateService.UpdateAsync(
            context.ChatId,
            UserStateType.AddMilestoneSelectingChild,
            data,
            ct);
    }

    private async Task HandleChildStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
    {
        logger.LogInformation("Processing childId for chat {ChatId}, preparing for category entering",
            context.ChatId);

        if (!int.TryParse(context.CallbackData, out var selectedChildId))
        {
            logger.LogWarning("Invalid callback data for child selection: {Data}", context.CallbackData);
            return;
        }

        var child = await parentRepository.GetByIdAsync(selectedChildId, ct);
        if (child == null)
        {
            logger.LogWarning("Child with ID {Id} not found", selectedChildId);
            throw new InvalidOperationException("Child not found");
        }

        var updatedData = data with { ChildId = child.Id, ChildName = child.Name };

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            $"Отлично! Добавляем воспоминание для <b>{child.Name}</b> ✨\n\n" +
            "Пожалуйста, <i>выберите категорию</i> из списка ниже:",
            BotKeyboards.CategorySelectionKeyboard(),
            ct);

        await userStateService.UpdateAsync(
            context.ChatId,
            UserStateType.AddMilestoneSelectingCategory,
            updatedData,
            ct);
    }

    private async Task HandleCategoryStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
    {
        logger.LogInformation("Processing category for chat {ChatId}, preparing for date entering",
            context.ChatId);

        if (!int.TryParse(context.CallbackData, out var selectedCategoryId)
            || !Enum.IsDefined(typeof(MilestoneCategory), selectedCategoryId))
        {
            logger.LogWarning("Invalid callback data for category selection: {Data}", context.CallbackData);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "⚠️ <b>Упс! Не удалось распознать категорию.</b>\n\n" +
                "Пожалуйста, выберите один из вариантов на кнопках выше. Если что-то идет не так, просто введите <b>/cancel</b>, чтобы начать заново.",
                BotKeyboards.CategorySelectionKeyboard(),
                ct);
            return;
        }

        var updatedData = data with { Category = (MilestoneCategory)selectedCategoryId };

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "Категория выбрана! ✅\n\n" +
            "Теперь укажите <b>дату этого события</b>. Вы можете нажать на кнопку ниже, если это произошло сегодня, или прислать дату сообщением в формате <code>ДД.ММ.ГГГГ</code> (например, 22.04.2026):",
            BotKeyboards.SelectCurrentDate(),
            ct);

        await userStateService.UpdateAsync(
            context.ChatId,
            UserStateType.AddMilestoneEnteringDate,
            updatedData,
            ct);
    }

    private async Task HandleDateStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
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
                return;
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
                return;
            }
        }

        var updatedData = data with { Date = parsedDate };

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            $"Дата события: <b>{parsedDate:dd.MM.yyyy}</b> ✅\n\n" +
            "Принято! Теперь придумайте <b>короткий заголовок</b> для этого воспоминания.\n\n" +
            "Например: <i>«Первое слово \"Мама\"»</i>, <i>«Первые шаги»</i> или <i>«Поход в зоопарк»</i>.\n\n" +
            "Если хотите оставить заголовок по умолчанию, нажмите кнопку <b>Пропустить</b> снизу 👇\n\n" +
            "Это поможет вам позже быстро найти событие в ленте.",
            BotKeyboards.SkipKeyboard(),
            ct: ct);

        await userStateService.UpdateAsync(
            context.ChatId,
            UserStateType.AddMilestoneEnteringTitle,
            updatedData,
            ct);
    }

    private async Task HandleTitleStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
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

        await messageService.SendTextMessageAsync(
            context.ChatId,
            $"{confirmationText}\n\n" +
            "Теперь самое важное — <b>опишите, что произошло</b>. 📝\n\n" +
            "Напишите подробности: какими были первые эмоции, что именно сказал или сделал ребенок, как это случилось.",
            ct: ct);

        await userStateService.UpdateAsync(
            context.ChatId,
            UserStateType.AddMilestoneEnteringDescription,
            updatedData,
            ct);
    }

    private async Task HandleDescriptionStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
    {
        logger.LogInformation("Processing description for chat {ChatId}, preparing for media upload",
            context.ChatId);

        if (string.IsNullOrWhiteSpace(context.Text))
        {
            logger.LogWarning("Description text is missing or empty for chat {ChatId}", context.ChatId);

            await messageService.SendTextMessageAsync(
                context.ChatId,
                "⚠️ <b>Описание не может быть пустым.</b>\n\n" +
                "Пожалуйста, напишите несколько слов о том, что произошло, чтобы сохранить это воспоминание:",
                ct: ct);
            return;
        }

        var updatedData = data with { Description = context.Text };

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "Текст принят! ✍️\n\n" +
            "Теперь вы можете <b>прислать фотографию или видео</b> к этому событию, чтобы оно стало ещё ярче.\n\n" +
            "Если не хотите добавлять медиа, просто нажмите кнопку <b>Пропустить</b> 👇",
            BotKeyboards.SkipKeyboard(),
            ct);

        await userStateService.UpdateAsync(
            context.ChatId,
            UserStateType.AddMilestoneUploadingMedia,
            updatedData,
            ct);
    }

    private async Task HandleMediaStep(BotContext context, CreateMilestoneData data, CancellationToken ct)
    {
        logger.LogInformation("Processing title for chat {ChatId}, preparing for final step",
            context.ChatId);

        bool isSkipped = context.IsCallback
                         && context.CallbackData == UiConstants.CallbackQueries.Skip;

        if (isSkipped)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "⏭️ Хорошо, пропускаем медиа.\n\nДавай посмотрим на итоговое воспоминание:",
                ct: ct);

            await SendMilestonePreviewAsync(context.ChatId, data, ct);

            await userStateService.UpdateAsync(
                context.ChatId,
                UserStateType.AddMilestoneConfirming,
                data,
                ct);

            return;
        }

        if (context.IsCallback && context.CallbackData == UiConstants.CallbackQueries.FinishMediaUpload)
        {
            if (data.MediaCount == 0)
            {
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    "❌ Ты ещё ничего не загрузил! Отправь фото/видео или нажми 'Пропустить'.",
                    BotKeyboards.SkipKeyboard(),
                    ct: ct);
                return;
            }

            if (data.MediaCount == 1)
            {
                var media = data.MediaGroup![0];

                if (media is InputMediaPhoto photo)
                {
                    await messageService.SendPhotoAsync(
                        context.ChatId,
                        ((InputFileId)photo.Media).Id,
                        $"✅ Отлично! Фото загружено.\n\nТеперь посмотрим на итоговое воспоминание:" +
                        data.GetSummary(data.ChildName),
                        ct: ct);

                    await SendMilestonePreviewAsync(context.ChatId, data, ct);

                    return;
                }

                if (media is InputMediaVideo video)
                {
                    await messageService.SendVideoAsync(
                        context.ChatId,
                        ((InputFileId)video.Media).Id,
                        $"✅ Отлично! Видео загружено.\n\nТеперь посмотрим на итоговое воспоминание:" +
                        data.GetSummary(data.ChildName),
                        ct: ct);

                    await SendMilestonePreviewAsync(context.ChatId, data, ct);

                    return;
                }
            }

            await messageService.SendTextMessageAsync(
                context.ChatId,
                $"✅ Отлично! Загружено {
                    data.MediaCount} {GetMediaWord(data.MediaCount)}.\n\nТеперь посмотрим на итоговое воспоминание:",
                ct: ct);

            var updatedData = data.AddCaption(data.GetSummary(data.ChildName));
            await SendMilestonePreviewAsync(context.ChatId, updatedData, ct);

            await userStateService.UpdateAsync(
                context.ChatId,
                UserStateType.AddMilestoneConfirming,
                updatedData,
                ct);
        }

        if (context.HasPhoto || context.HasVideo)
        {
            await StateLock.WaitAsync(ct);
            CreateMilestoneData latestData;
            try
            {
                var currentState = await userStateService.GetAsync(context.ChatId, ct);
                latestData = JsonSerializer.Deserialize<CreateMilestoneData>(currentState?.StateData ?? "{}") ?? data;
                latestData = context.HasPhoto
                    ? latestData.AddPhoto(context.PhotoFileId!)
                    : latestData.AddVideo(context.VideoFileId!);
                await userStateService.UpdateAsync(
                    context.ChatId,
                    UserStateType.AddMilestoneUploadingMedia,
                    latestData,
                    ct);
            }
            finally
            {
                StateLock.Release();
            }

            if (!string.IsNullOrEmpty(context.MediaGroupId))
            {
                await Task.Delay(800, ct); // waiting till all the media come 

                // checking for new data
                var checkState = await userStateService.GetAsync(context.ChatId, ct);
                var checkData = JsonSerializer.Deserialize<CreateMilestoneData>(checkState?.StateData ?? "{}");

                // if database have more media - this message not latest
                if (checkData?.MediaCount > latestData.MediaCount)
                {
                    return;
                }
            }

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                $"{latestData.MediaCount} {GetMediaWord(latestData.MediaCount)} принято! ✍️\n\n" +
                "Ecли хотите завершить загрузку медиа нажмите <b>Завершить</b>.\n\n" +
                "Для загрузки дополнительных медиа просто пришлите их сюда",
                BotKeyboards.MediaUploadKeyboard(latestData.MediaCount),
                ct);
        }
    }

    private async Task HandleConfirmingAsync(BotContext context, CreateMilestoneData data, CancellationToken ct)
    {
        logger.LogInformation("Processing title for chat {ChatId}, preparing for final step",
            context.ChatId);

        if (!context.IsCallback)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "Неизвестное сообщение. Пожалуйста нажмите на кнопку ниже чтобы подтвердить создание, отменить или изменить отдельные элементы.",
                ct: ct);
            return;
        }

        var callback = context.CallbackData;

        if (callback == UiConstants.CallbackQueries.EditMilestone.Confirm)
        {
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

            await userStateService.UpdateAsync(context.ChatId, nextState, data, ct);
        }
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

    private async Task SendMilestonePreviewAsync(long chatId, CreateMilestoneData data, CancellationToken ct)
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