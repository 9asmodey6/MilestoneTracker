namespace MilestoneTracker.Application.Common.Features.Children.DeleteChild.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Models;
using Shared.State;

public class ConfirmDeleteChildStepHandler(
    IParentRepository parentRepository,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ILogger<ConfirmDeleteChildStepHandler> logger) : IStepHandler<DeleteChildData>
{
    public UserStateType Step => UserStateType.DeleteChildConfirming;

    public async Task<StepResult<DeleteChildData>> HandleAsync(BotContext context, DeleteChildData data,
        CancellationToken ct)
    {
        logger.LogInformation("Performing delete child operation for chat {chatId}", context.ChatId);

        if (!context.IsCallback)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "Неизвестный ввод. Пожалуйста, выбери действие или нажми /cancel",
                ct: ct);

            return new StepResult<DeleteChildData>(UserStateType.DeleteChildConfirming, data);
        }

        // asking for confirming
        if (context.IsCallback && (context.Payload != null || (context.CallbackData != null &&
                                                               context.CallbackData.StartsWith(UiConstants
                                                                   .CallbackQueries.DeleteChild.DeleteChildPrefix))))
        {
            var rawId = context.Payload ?? context.CallbackData!.Replace(
                UiConstants.CallbackQueries.DeleteChild.DeleteChildPrefix,
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
                    return new StepResult<DeleteChildData>(UserStateType.Idle, null);
                }

                var updatedData = new DeleteChildData(ChildId: child.Id, ChildName: child.Name);

                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    $"🗑 <b>Удаление профиля ребёнка</b>\n\n" +
                    $"Вы уверены, что хотите удалить профиль <b>{child.Name}</b>?\n\n" +
                    $"⚠️ <b>Это действие приведёт к следующим последствиям:</b>\n" +
                    $"• Все воспоминания, связанные с ребёнком, будут скрыты.\n" +
                    $"• Доступ к профилю для других пользователей будет заблокирован.\n\n" +
                    $"<i>Вы сможете восстановить данные в течение некоторого времени после удаления.</i>",
                    BotKeyboards.ChildDeleteConfirmationKeyboard(),
                    ct: ct);

                return new StepResult<DeleteChildData>(UserStateType.DeleteChildConfirming, updatedData);
            }
        }

        // delete child confirmed
        if (context.CallbackData == UiConstants.CallbackQueries.DeleteChild.DeleteChildConfirmed)
        {
            if (data.ChildId == null)
            {
                logger.LogError("ChildId is missing in data for chat {chatId}", context.ChatId);
                await messageService.SendTextMessageAsync(
                    context.ChatId,
                    "⚠️ <b>Произошла ошибка!</b>\n\nПожалуйста попробуйтё ещё раз через главное меню!",
                    ct: ct);
                await userStateService.ResetAsync(context.ChatId, ct);
                return new StepResult<DeleteChildData>(UserStateType.Idle, null);
            }

            var child = await parentRepository.GetChildByIdAsync(data.ChildId.Value, ct);
            if (child == null)
            {
                logger.LogError("ChildId is missing in data for chat {chatId}", context.ChatId);
                await messageService.SendTextMessageAsync(
                    context.ChatId,
                    "⚠️ <b>Произошла ошибка!</b>\n\nПожалуйста попробуйтё ещё раз через главное меню!",
                    ct: ct);
                await userStateService.ResetAsync(context.ChatId, ct);
                return new StepResult<DeleteChildData>(UserStateType.Idle, null);
            }

            var isSuccess = await parentRepository.SoftDeleteAsync(context.ChatId, data.ChildId.Value, ct) == 1;

            if (!isSuccess)
            {
                logger.LogError("Error while processing soft child deletion for chat {chatId}", context.ChatId);
                await messageService.SendTextMessageAsync(
                    context.ChatId,
                    "⚠️ <b>Произошла ошибка!</b>\n\nПожалуйста попробуйтё ещё раз через главное меню!",
                    ct: ct);
                await userStateService.ResetAsync(context.ChatId, ct);
                return new StepResult<DeleteChildData>(UserStateType.Idle, null);
            }

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                $"⚠️ <b>Ребёнок {data.ChildName} удалён</b>\n\nВы сможете восстановить его и все его восспоминания <i>из меню или нажав кнопку ниже</i>",
                BotKeyboards.UndoChildDeletionKeyboard(data.ChildId.Value),
                ct);

            await userStateService.ResetAsync(context.ChatId, ct);
        }

        return new StepResult<DeleteChildData>(UserStateType.Idle, null);
    }
}