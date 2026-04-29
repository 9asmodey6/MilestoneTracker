namespace MilestoneTracker.Application.Common.Features.Children.ProvideAccess.Steps;

using Domain.Enums;
using Infrastructure.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Interfaces;
using Shared.Models;
using Shared.State;

public class ConfirmProvideStepHandler(
    IMediator mediator,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    IParentRepository parentRepository,
    ILogger<ConfirmProvideStepHandler> logger) : IStepHandler<ProvideAccessData>
{
    public UserStateType Step => UserStateType.ProvideAccessConfirming;

    public async Task<StepResult<ProvideAccessData>> HandleAsync(BotContext context, ProvideAccessData data, CancellationToken ct)
    {
        if (!context.IsCallback)
        {
             await messageService.SendTextMessageAsync(
                context.ChatId,
                "Пожалуйста, используйте кнопки для подтверждения или напишите /cancel для отмены.",
                ct: ct);
            return new StepResult<ProvideAccessData>(Step, data);
        }

        if (!int.TryParse(context.CallbackData, out var childId))
        {
             logger.LogWarning("Invalid callback data for ProvideAccessConfirming: {CallbackData}", context.CallbackData);
             return new StepResult<ProvideAccessData>(Step, data);
        }

        var parent = await parentRepository.GetWithChildrenAsync(context.ChatId, ct);
        if (parent == null)
        {
             await messageService.SendTextMessageAsync(context.ChatId, "❌ Ошибка: родитель не найден в системе.", ct: ct);
             await userStateService.ResetAsync(context.ChatId, ct);
             return new StepResult<ProvideAccessData>(UserStateType.Idle, null);
        }

        logger.LogInformation("Generating access token for child {ChildId} by parent {ParentId}", childId, parent.Id);

        var command = new GenerateAccessTokenCommand(parent.Id, childId);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                $"✅ <b>Токен успешно сгенерирован!</b>\n\n" +
                $"Передайте этот код второму родителю:\n\n" +
                $"<code>{result.Token}</code>\n\n" +
                $"Этот код действителен в течение 24 часов.",
                ct: ct);
            
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<ProvideAccessData>(UserStateType.Idle, null);
        }
        else
        {
            await messageService.SendTextMessageAsync(
                context.ChatId,
                $"❌ <b>Не удалось создать токен:</b>\n{result.ErrorMessage}",
                ct: ct);
            
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<ProvideAccessData>(UserStateType.Idle, null);
        }
    }
}