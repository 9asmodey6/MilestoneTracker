namespace MilestoneTracker.Application.Common.Features.Children.GainAccess;

using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Services;

public class GainAccessByTokenHandler(
    IChildAccessTokenService accessTokenService,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ILogger<GainAccessByTokenHandler> logger) 
    : IRequestHandler<GainAccessByTokenCommand>
{
    public async Task Handle(GainAccessByTokenCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing token consumption for chat {ChatId}. Token: {Token}", 
            request.ParentChatId, request.Token);

        var result = await accessTokenService.ConsumeTokenAsync(request.Token!, request.ParentChatId, cancellationToken);
        
        if (result.IsFailure)
        {
            logger.LogWarning("Token consumption failed for chat {ChatId}. Reason: {Reason}", 
                request.ParentChatId, result.ErrorMessage);

            await messageService.SendTextMessageAsync(
                request.ParentChatId,
                $"❌ <b>Ошибка доступа</b>\n\n" +
                $"<i>{result.ErrorMessage}</i>\n\n" +
                $"Пожалуйста, проверьте правильность ввода кода или обратитесь ко второму родителю за новым кодом.",
                ct: cancellationToken);
            
            await userStateService.ResetAsync(request.ParentChatId, cancellationToken);
            return;
        }

        logger.LogInformation("Successfully consumed token for chat {ChatId}. Access granted.", request.ParentChatId);

        await messageService.SendTextMessageAsync(
            request.ParentChatId,
            $"🎉 <b>Поздравляем!</b>\n\n" +
            $"Вы успешно получили доступ к записям о ребёнке. Теперь вы можете просматривать и добавлять новые воспоминания вместе!\n\n" +
            $"Используйте меню ниже, чтобы управлять записями.",
            BotKeyboards.MainMenuKeyboard,
            ct: cancellationToken);

        await userStateService.ResetAsync(request.ParentChatId, cancellationToken);
    }
}
