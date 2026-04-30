namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Commands.Bot.Start;
using Application.Common.Commands.BotCommands.Cancel;
using Application.Common.Commands.BotCommands.Help;
using Application.Common.Constants;
using Application.Common.Features.Children.GetChildren;
using Application.Common.Interfaces;
using Application.Common.Shared.State;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Telegram.Bot;
using Telegram.Bot.Types;

public class UpdateHandler(
    IMediator mediator,
    ILogger<UpdateHandler> logger,
    IUserStateService stateService,
    UserFlowHandlerFactory handlerFactory,
    ITelegramBotClient botClient)
{
    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        var context = BotContext.FromUpdate(update);
        if (context == null)
        {
            logger.LogError("Failed to parse BotContext from Update {UpdateId}", update.Id);
            return;
        }

        logger.LogInformation("Processing update for ChatId: {ChatId}", context.ChatId);

        try
        {
            if (context.IsCallback)
            {
                await botClient.AnswerCallbackQuery(context.CallbackQueryId!, cancellationToken: ct);
            }

            if (context.Text?.StartsWith('/') ?? false)
            {
                await HandleCommandAsync(context, ct);
                return;
            }

            if (context.IsCallback)
            {
                await HandleCallbackQueryAsync(context, ct);
                return;
            }

            var state = await stateService.GetAsync(context.ChatId, ct);

            if (IsMenuButton(context.Text))
            {
                await HandleMenuButtonAsync(context, state, ct);
                return;
            }

            if (state.State != UserStateType.Idle)
            {
                await HandleStatefulInteractionAsync(context, state, ct);
                return;
            }

            logger.LogWarning("Unhandled input from {ChatId}: {Text}", context.ChatId, context.Text);
            await botClient.SendMessage(context.ChatId, 
                "😕 Я не совсем понял, что вы имели в виду. Пожалуйста, воспользуйтесь кнопками меню или введите /help для справки.", 
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update from {ChatId}", context.ChatId);
            await botClient.SendMessage(context.ChatId, 
                "❌ Произошла ошибка при обработке вашего запроса. Пожалуйста, попробуйте еще раз или используйте команду /cancel.", 
                cancellationToken: ct);
        }
    }

    private async Task HandleCallbackQueryAsync(
        BotContext context,
        CancellationToken ct)
    {
        logger.LogInformation("DEBUG: CallbackData received: {Data}", context.Text);

        var state = await stateService.GetAsync(context.ChatId, ct);
        var data = context.Text;

        if (string.IsNullOrEmpty(data))
        {
            logger.LogWarning("Empty callback query from {ChatId}", context.ChatId);
            return;
        }

        // Handle legacy/inline specific data
        if (data == UiConstants.CallbackQueries.AddChild)
        {
            data = UiConstants.ReplyButtons.AddChild;
        }

        // If callback data matches a menu button, redirect to HandleMenuButtonAsync
        if (IsMenuButton(data))
        {
            await HandleMenuButtonAsync(context with { Text = data }, state, ct);
            return;
        }

        var handler = handlerFactory.GetHandler(state.State);
        if (handler != null)
        {
            await handler.HandleAsync(context, state, ct);
        }
        else
        {
            logger.LogWarning("No handler found for state {State} and callback data {Data}", state.State, data);
            await botClient.SendMessage(context.ChatId, 
                "😕 Не удалось обработать ваш выбор. Пожалуйста, используйте кнопки меню или введите /help.", 
                cancellationToken: ct);
        }
    }

    private async Task HandleCommandAsync(BotContext context, CancellationToken ct)
    {
        switch (context.Text)
        {
            case "/start":
                await mediator.Send(new StartCommand(
                    context.ChatId,
                    context.FirstName,
                    context.Username), ct);
                break;

            case "/cancel":
                await mediator.Send(new CancelCommand(context.ChatId), ct);
                break;
                
            case "/help":
                await mediator.Send(new HelpCommand(context.ChatId), ct);
                break;
                
            default:
                await botClient.SendMessage(context.ChatId, 
                    "❓ Неизвестная команда. Введите /help, чтобы увидеть список доступных команд.", 
                    cancellationToken: ct);
                break;
        }
    }

    private async Task HandleStatefulInteractionAsync(BotContext context, UserState state, CancellationToken ct)
    {
        var handler = handlerFactory.GetHandler(state.State);
        if (handler != null)
        {
            await handler.HandleAsync(context, state, ct);
        }
        else
        {
            logger.LogWarning("No handler found for state {State} for chat {ChatId}", state.State, context.ChatId);
            await stateService.ResetAsync(context.ChatId, ct);
            await botClient.SendMessage(context.ChatId, 
                "😕 Что-то пошло не так с текущим действием. Я сбросил состояние, пожалуйста, попробуйте еще раз.", 
                cancellationToken: ct);
        }
    }

    private async Task HandleMenuButtonAsync(
        BotContext context,
        UserState state,
        CancellationToken ct)
    {
        switch (context.Text)
        {
            case UiConstants.ReplyButtons.AddChild:
                var addChildHandler = handlerFactory.GetHandler(UserStateType.AddChildStarted);
                if (addChildHandler != null)
                {
                    state.State = UserStateType.AddChildStarted;
                    await addChildHandler.HandleAsync(context, state, ct);
                }
                break;
            case UiConstants.ReplyButtons.MyChildren:
               await mediator.Send(new GetChildrenQuery(
                    context.ChatId), ct);
                break;
            case UiConstants.ReplyButtons.AddMilestone:
                var addMilestoneHandler = handlerFactory.GetHandler(UserStateType.AddMilestoneStarted);
                if (addMilestoneHandler != null)
                {
                    state.State = UserStateType.AddMilestoneStarted;
                    await addMilestoneHandler.HandleAsync(context, state, ct);
                }
                break;
            case  UiConstants.ReplyButtons.ViewMilestones:
                var getMilestoneHandler = handlerFactory.GetHandler(UserStateType.GetMilestoneSelectingChild);
                if (getMilestoneHandler != null)
                {
                    state.State = UserStateType.GetMilestoneSelectingChild;
                    await getMilestoneHandler.HandleAsync(context, state, ct);
                }
                break;
            case UiConstants.ReplyButtons.ProvideAccessByToken:
                state.State = UserStateType.ProvideAccessSelectingChild;
                var provideAccessHandler = handlerFactory.GetHandler(state.State);
                if (provideAccessHandler != null)
                {
                    await provideAccessHandler.HandleAsync(context, state, ct);
                }
                break;
            case UiConstants.ReplyButtons.GainAccessByToken:
                state.State = UserStateType.GainAccessEnteringToken;
                var gainAccessHandler = handlerFactory.GetHandler(state.State);
                if (gainAccessHandler != null)
                {
                    await gainAccessHandler.HandleAsync(context, state, ct);
                }
                break;
            case UiConstants.ReplyButtons.Help:
                await mediator.Send(new HelpCommand(context.ChatId), ct);
                break;
        }
    }

    private bool IsMenuButton(string? text) => text switch
    {
        UiConstants.ReplyButtons.AddChild => true,
        UiConstants.ReplyButtons.AddMilestone => true,
        UiConstants.ReplyButtons.MyChildren => true,
        UiConstants.ReplyButtons.ViewMilestones => true,
        UiConstants.ReplyButtons.ProvideAccessByToken => true,
        UiConstants.ReplyButtons.GainAccessByToken => true,
        UiConstants.ReplyButtons.Help => true,
        _ => false
    };
}