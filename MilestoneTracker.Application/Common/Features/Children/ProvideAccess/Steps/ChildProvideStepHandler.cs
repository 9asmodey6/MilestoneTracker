namespace MilestoneTracker.Application.Common.Features.Children.ProvideAccess.Steps;

using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.State;

public class ChildProvideStepHandler(
    ITelegramMessageService messageService,
    IParentRepository parentRepository,
    IUserStateService userStateService,
    IChildAccessTokenService tokenService,
    ILogger<ChildProvideStepHandler> logger) : IStepHandler<ProvideAccessData>
{
    public UserStateType Step => UserStateType.ProvideAccessSelectingChild;

    public async Task<StepResult<ProvideAccessData>> HandleAsync(BotContext context, ProvideAccessData data,
        CancellationToken ct)
    {
        logger.LogDebug("Provide Access Token started. Processing child selection for chat {ChatId}.",
            context.ChatId);

        var children = await parentRepository.GetChildrenAsync(context.ChatId, ct);

        if (children.Count == 0)
        {
            logger.LogDebug("Children not found for chat {ChatId}", context.ChatId);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "<b>В базе пока нет ваших детей.</b>\n\nНажмите кнопку ниже, чтобы добавить первого ребенка и начать отслеживать его достижения! 👇",
                BotKeyboards.AddChildKeyboard,
                ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<ProvideAccessData>(UserStateType.Idle, null);
        }
        
        if (children.Count == 1)
        {
            var tokens = await tokenService.GetChildAccessTokensAsync(children[0].Id, ct);
            
            if (tokens.Count >= 1)
            {
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    $"⚠️<b>Вы уже создали код доступа к этому ребёнку</b>\n\n" +
                    "Поделитесь сущевствующим кодом либо нажмите кнопку ниже чтобы <i>отозвать его</i>",
                    BotKeyboards.RecallTokenKeyboard(tokens[0].Id),
                    ct);

                await userStateService.ResetAsync(context.ChatId, ct);
                return new StepResult<ProvideAccessData>(UserStateType.Idle, null);
            }
            
            var child = children[0];
            var updatedData = data with { ChildId = child.Id, ChildName = child.Name };

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                $"Делимся доступом для ребёнка <b>{child.Name}</b> ✨\n\n" +
                "Если согласны,нажмите <i>кнопку ниже</i>",
                BotKeyboards.ConfirmChildForProvidingKeyboard(child),
                ct);

            return new StepResult<ProvideAccessData>(UserStateType.ProvideAccessConfirming, updatedData);
        }
        
        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "Доступом к какому ребёнку вы хотите поделиться? Пожалуйста, <i>выберите ребенка</i> из списка ниже:",
            BotKeyboards.ChildSelectionKeyboard(children.ToList()),
            ct);
        
        return  new StepResult<ProvideAccessData>(UserStateType.ProvideAccessConfirming, data);
    }
}