namespace MilestoneTracker.Application.Common.Features.Children.AddChild;

using System.Text.Json;
using Constants;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Interfaces;
using Shared.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.CalendarKit;
using Telegram.CalendarKit.Models.Enums;

public class ProcessChildStepHandler(
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ITelegramCalendarParser calendarParser,
    CalendarBuilder calendarBuilder,
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
                await HandleAgeStep(userState, data, ct);
                break;
            case UserStateType.AddChildUploadingPhoto:
                await HandlePhotoStep(userState, data, ct);
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
        
        var calendarButtons = calendarBuilder.GenerateCalendarButtons(
            DateTime.Now.Year, 
            DateTime.Now.Month, 
            CalendarViewType.Default, 
            "ru");

        await messageService.SendTextMessageAsync(
            context.ChatId,
            $"Принято! Имя: **{updatedData.Name}**\n\nТеперь выбери дату рождения ребёнка:",
            replyMarkup: calendarButtons,
            ct);

        await userStateService.UpdateAsync<CreateChildData>(
            context.ChatId,
            UserStateType.AddChildEnteringName,
            data,
            ct);
    }
}