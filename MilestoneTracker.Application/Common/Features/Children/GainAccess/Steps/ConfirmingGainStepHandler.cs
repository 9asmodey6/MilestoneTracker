namespace MilestoneTracker.Application.Common.Features.Children.GainAccess.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Models;
using Shared.State;

public class ConfirmingGainStepHandler(
    IMediator mediator,
    ITelegramMessageService messageService,
    ILogger<ConfirmingGainStepHandler> logger) : IStepHandler<GainAccessByTokenCommand>
{
    public UserStateType Step => UserStateType.GainAccessConfirming;

    public async Task<StepResult<GainAccessByTokenCommand>> HandleAsync(BotContext context, GainAccessByTokenCommand data, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(context.Text))
        {
            logger.LogWarning("Parent {ChatId} sent an empty message instead of a token.", context.ChatId);
            
            await messageService.SendTextMessageAsync(
                context.ChatId,
                "⚠️ <b>Ожидается код доступа</b>\n\n" +
                "Пожалуйста, пришлите текстовый код или нажмите /cancel для отмены.",
                ct: ct);
            
            return new StepResult<GainAccessByTokenCommand>(Step, data);
        }

        logger.LogInformation("Parent {ChatId} provided token for validation.", context.ChatId);

        await mediator.Send(data with { Token = context.Text.Trim() }, ct);
        return new StepResult<GainAccessByTokenCommand>(UserStateType.Idle, null);
    }
}