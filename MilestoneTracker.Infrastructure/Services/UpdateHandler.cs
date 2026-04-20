namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Commands.Bot.Start;
using Application.Common.Commands.BotCommands.Cancel;
using Application.Common.Commands.State;
using Application.Common.Constants;
using Application.Common.Features.Children.GetChildren;
using Application.Common.Interfaces;
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

        if (context.IsCallback)
        {
            await botClient.AnswerCallbackQuery(context.CallbackQueryId!, cancellationToken: ct);
            await HandleCallbackQueryAsync(context, ct);
            return;
        }

        if (context.Text?.StartsWith('/') ?? false)
        {
            await HandleCommandAsync(context, ct);
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
            logger.LogWarning("Unhandled callback query from {ChatId}: {Data}", context.ChatId, data);
            return;
        }

        var handler = handlerFactory.GetHandler(state.State);
        await handler.HandleAsync(context, state, ct);
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
                
            default:
                throw new InvalidOperationException();
                // await HandleUnknownCommand(context.ChatId, ct);
                break;
        }
    }

    private async Task HandleStatefulInteractionAsync(BotContext context, UserState state, CancellationToken ct)
    {
        var handler = handlerFactory.GetHandler(state.State);
        await handler.HandleAsync(context, state, ct);
    }

    private async Task HandleMenuButtonAsync(
        BotContext context,
        UserState state,
        CancellationToken ct)
    {
        switch (context.Text)
        {
            case UiConstants.ReplyButtons.AddChild:
                var handler = handlerFactory.GetHandler(UserStateType.AddChildStarted);
                state.State = UserStateType.AddChildStarted;
                await handler.HandleAsync(context, state, ct);
                break;
            case UiConstants.ReplyButtons.MyChildren:
               await mediator.Send(new GetChildrenQuery(
                    context.ChatId), ct);
                break;
        }
    }

    private bool IsMenuButton(string? text) => text switch
    {
        UiConstants.ReplyButtons.AddChild => true,
        UiConstants.ReplyButtons.AddMilestone => true,
        UiConstants.ReplyButtons.MyChildren => true,
        UiConstants.ReplyButtons.History => true,
        UiConstants.ReplyButtons.Help => true,
        _ => false
    };
}