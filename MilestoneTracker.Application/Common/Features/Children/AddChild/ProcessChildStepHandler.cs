namespace MilestoneTracker.Application.Common.Features.Children.AddChild;

using System.Text.Json;
using Constants;
using Domain.Entities;
using Domain.Entities.ValueObjects;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Interfaces;
using Shared.Bot.Keyboards;
using Shared.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

public class ProcessChildStepHandler(
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ITelegramDateParser dateParser,
    IMediator mediator,
    IParentRepository parentRepository,
    ITelegramBotClient botClient,
    ILogger<ProcessChildStepHandler> logger) : IUserFlowHandler
{
    public bool CanHandle(UserStateType userState) =>
        userState >= UserStateType.AddChildStarted
        && userState <= UserStateType.AddChildUploadingPhoto;

    public async Task HandleAsync(BotContext context, UserState userState, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<CreateChildData>(userState?.StateData ?? "{}")
                   ?? new CreateChildData();

        switch (userState!.State)
        {
            case UserStateType.AddChildStarted:
                await HandleStartedStep(context, data, ct);
                break;
            case UserStateType.AddChildEnteringName:
                await HandleNameStep(context, data, ct);
                break;
            case UserStateType.AddChildEnteringBirthdate:
                await HandleBirthdayStep(context, data, ct);
                break;
            case UserStateType.AddChildUploadingPhoto:
                await HandlePhotoStep(context, data, ct);
                break;
        }
    }

    private async Task HandleStartedStep(BotContext context, CreateChildData data, CancellationToken ct)
    {
        logger.LogInformation("Started child adding step for chat {ChatId}, waiting for name entering",
            context.ChatId);

        await messageService.SendTextMessageAsync(
            context.ChatId,
            "Отлично! Начинаем. 👶\n\nДля начала, **как зовут ребёнка?**",
            replyMarkup: new ReplyKeyboardRemove(),
            ct);

        await userStateService.UpdateAsync<CreateChildData>(
            context.ChatId,
            UserStateType.AddChildEnteringName,
            null,
            ct);
    }

    private async Task HandleNameStep(BotContext context, CreateChildData data, CancellationToken ct)
    {
        logger.LogInformation("Started child adding step for chat {ChatId}, waiting for age entering",
            context.ChatId);

        var updatedData = data with { Name = context.Text };

        await messageService.SendTextMessageAsync(
            context.ChatId,
            $"Принято! Имя: **{updatedData.Name}**\n\nТеперь введи дату рождения ребёнка в формате\n**ДД.ММ.ГГГГ**:",
            ct: ct);

        await userStateService.UpdateAsync<CreateChildData>(
            context.ChatId,
            UserStateType.AddChildEnteringBirthdate,
            updatedData,
            ct);
    }

    private async Task HandleBirthdayStep(BotContext context, CreateChildData data, CancellationToken ct)
    {
        logger.LogInformation("Started child adding step for chat {ChatId}, waiting for photo entering",
            context.ChatId);

        var birthDate = dateParser.ParseDate(context.Text!);

        if (birthDate == null)
        {
            logger.LogWarning("Failed to parse birthdate from input: {Input}", context.Text);

            await messageService.SendTextMessageAsync(
                context.ChatId,
                "⚠️ **Неверный формат даты.**\n\nПожалуйста, напиши дату рождения в формате: **ДД.ММ.ГГГГ**\nНапример: `20.04.2023`",
                ct: ct);
            return;
        }

        var updatedData = data with
        {
            BirthDate = birthDate
        };

        await messageService.SendTextMessageAsync(
            context.ChatId,
            $"Отлично! Дата рождения: {birthDate.Value:dd.MM.yyyy}\n\nТеперь отправь фото малыша (или нажми кнопку ниже, чтобы пропустить):",
            replyMarkup: BotKeyboards.SkipPhotoKeyboard(),
            ct: ct);

        await userStateService.UpdateAsync<CreateChildData>(
            context.ChatId,
            UserStateType.AddChildUploadingPhoto,
            updatedData,
            ct);
    }

    private async Task HandlePhotoStep(BotContext context, CreateChildData data, CancellationToken ct)
    {
        logger.LogInformation("Started child adding step for chat {ChatId}, waiting for photo",
            context.ChatId);

        bool isSkipped = context.IsCallback
                         && context.CallbackData == UiConstants.CallbackQueries.SkipPhoto;

        if (!isSkipped && !context.HasPhoto)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "Пожалуйста, отправь фото или нажми на кнопку «Пропустить»! 👇",
                BotKeyboards.SkipPhotoKeyboard(),
                ct: ct);
            return;
        }

        if (string.IsNullOrEmpty(data.Name) || data.BirthDate == null)
        {
            logger.LogError("Invalid data when creating child. Name: {Name}, BirthDate: {Date}",
                data.Name, data.BirthDate);

            await messageService.SendTextMessageAsync(
                context.ChatId,
                "❌ Ошибка! Данные потеряны. Начни заново с кнопки '➕ Добавить ребёнка'",
                ct: ct);

            await userStateService.ResetAsync(context.ChatId, ct);
            return;
        }

        var parent = await parentRepository.GetAsync(context.ChatId, ct);
        if (parent == null)
        {
            logger.LogError("Parent not found for ChatId: {ChatId}", context.ChatId);
            return;
        }

        var photoId = isSkipped ? null : context.PhotoFileId;

        await mediator.Send(new CreateChildCommand(
            parent.Id,
            data.Name,
            data.BirthDate!.Value,
            photoId), ct);

        if (context.IsCallback)
        {
            await botClient.EditMessageReplyMarkup(
                context.ChatId,
                context.MessageId!.Value,
                replyMarkup: null,
                cancellationToken: ct);
        }

        await messageService.SendTextMessageAsync(
            context.ChatId,
            $"🎉 {data.Name} успешно добавлен!\nСейчас малышу {AgeInfo.Calculate(data.BirthDate.Value, DateTime.UtcNow)}",
            BotKeyboards.MainMenuKeyboard,
            ct);

        await userStateService.ResetAsync(context.ChatId, ct);
    }
}