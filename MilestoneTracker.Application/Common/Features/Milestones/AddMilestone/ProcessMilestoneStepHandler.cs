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
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

public class ProcessMilestoneStepHandler(
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    IParentRepository parentRepository,
    ILogger<ProcessMilestoneStepHandler> logger) : IUserFlowHandler
{
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
            title = context.Text;
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
            var child = await parentRepository.GetByIdAsync(data.ChildId!.Value, ct);

            await messageService.SendTextMessageAsync(
                context.ChatId,
                "⏭️ Хорошо, пропускаем медиа.\n\nДавай посмотрим на итоговое воспоминание:" +
                data.ToString(child!.Name),
                ct: ct);

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

            await messageService.SendTextMessageAsync(
                context.ChatId,
                $"✅ Отлично! Загружено {data.MediaCount} {GetMediaWord(data.MediaCount)}.\n\nТеперь посмотрим на итоговое воспоминание:",
                ct: ct);
            
            
            
            await userStateService.UpdateAsync(
                context.ChatId,
                UserStateType.AddMilestoneConfirming,
                data,
                ct);
        }

        if (context.HasPhoto)
        {
            data = data.AddPhoto(context.PhotoFileId!);
        }

        if (context.HasVideo)
        {
            data = data.AddVideo(context.VideoFileId!);
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
}