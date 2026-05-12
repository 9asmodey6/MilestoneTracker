namespace MilestoneTracker.Application.Common.Features.Children.GetChildren.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.Services;
using Shared.State;

public class ViewChildStepHandler(
    ITelegramMessageService messageService,
    IChildViewService childViewService,
    IParentRepository parentRepository,
    IUserStateService userStateService,
    ILogger<ViewChildStepHandler> logger) : IStepHandler<GetChildrenData>
{
    public UserStateType Step => UserStateType.GetChildrenViewItem;

    public async Task<StepResult<GetChildrenData>> HandleAsync(BotContext context, GetChildrenData data,
        CancellationToken ct)
    {
        logger.LogInformation("Handling child input ChatId: {ChatId}", context.ChatId);

        if (data.ChildId != null)
        {
            var child = await parentRepository.GetChildByIdAsync(data.ChildId.Value, ct);
            await childViewService.SendChildCardAsync(
                context.ChatId,
                child,
                BotKeyboards.SelectChildActionKeyboard(child.Id.ToString()), ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<GetChildrenData>(UserStateType.Idle, null);
        }

        if (!context.IsCallback)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "Неизвестный ввод. Пожалуйста, выбери ребёнка из списка или нажми /cancel",
                ct: ct);

            return new StepResult<GetChildrenData>(UserStateType.GetChildrenViewItem, data);
        }

        if (context.IsCallback && (context.Payload != null || (context.CallbackData != null &&
            context.CallbackData.StartsWith(UiConstants.CallbackQueries.GetChild.GetChildPrefix))))
        {
            var rawId = context.Payload ?? context.CallbackData!.Replace(UiConstants.CallbackQueries.GetChild.GetChildPrefix,
                        string.Empty,
                        StringComparison.Ordinal);

            if (int.TryParse(rawId, out int itemId))
            {
                var child = await parentRepository.GetChildByIdAsync(itemId, ct);
                if (child == null)
                {
                    logger.LogError("Child {childId} not found for chat {chatId}", itemId, context.ChatId);
                    await messageService.SendTextMessageAsync(
                        context.ChatId,
                        "⚠️ <b>Произошла ошибка!</b>\n\nПожалуйста попробуйтё ещё раз через главное меню!",
                        ct: ct);
                    await userStateService.ResetAsync(context.ChatId, ct);
                    return new StepResult<GetChildrenData>(UserStateType.Idle, null);
                }

                await childViewService.SendChildCardAsync(
                    context.ChatId,
                    child,
                    BotKeyboards.SelectChildActionKeyboard(child.Id.ToString()),
                    ct);

                await userStateService.ResetAsync(context.ChatId);
                return new StepResult<GetChildrenData>(UserStateType.Idle, null);
            }
        }

        return new StepResult<GetChildrenData>(UserStateType.GetChildrenViewItem, data);
    }
}