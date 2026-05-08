namespace MilestoneTracker.Application.Common.Features.Children.GetChildren.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;
using Shared.Models;
using Shared.Services;
using Shared.State;

public class SelectingChildStepHandler(
    ITelegramMessageService messageService,
    ILogger<SelectingChildStepHandler> logger,
    IServiceProvider serviceProvider,
    IParentRepository repository) : IStepHandler<GetChildrenData>
{
    public UserStateType Step => UserStateType.GetChildrenSelecting;

    public async Task<StepResult<GetChildrenData>> HandleAsync(BotContext context, GetChildrenData data,
        CancellationToken ct)
    {
        logger.LogInformation("Fetching children for ChatId: {ChatId}", context.ChatId);

        var updatedData = data with { ChatId = context.ChatId };

        var children = await repository.GetChildrenAsync(context.ChatId, ct);
        if (!children.Any())
        {
            logger.LogInformation("No children found for ChatId: {ChatId}", context.ChatId);

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "<b>В базе пока нет ваших детей.</b>\n\nНажмите кнопку ниже, чтобы добавить первого ребенка и начать отслеживать его достижения! 👇",
                BotKeyboards.AddChildKeyboard,
                ct);

            return new StepResult<GetChildrenData>(UserStateType.Idle, null);
        }

        if (children.Count == 1)
        {
            var child = children[0];

            var viewHandler = serviceProvider
                .GetServices<IStepHandler<GetChildrenData>>()
                .First(h => h.Step == UserStateType.GetChildrenViewItem);

            return await viewHandler.HandleAsync(context, updatedData with { ChildId = child.Id }, ct);
        }

        var text = ChildListMessageBuilder.BuildListMessage("👶 Выберите ребёнка", children);
        var keyboard = BotKeyboards.NumberedChildSelectionKeyboard(
            children,
            UiConstants.CallbackQueries.GetChild.GetChildPrefix,
            UiConstants.CallbackQueries.GetChild.GetChildBackToList);

        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            text,
            keyboard,
            ct);

        return new StepResult<GetChildrenData>(UserStateType.GetChildrenViewItem, updatedData);
    }
}