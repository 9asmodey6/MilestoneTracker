namespace MilestoneTracker.Application.Common.Commands.Bot.Start;

using MediatR;
using Microsoft.Extensions.Logging;
using MilestoneTracker.Application.Common.Interfaces;
using MilestoneTracker.Domain.Entities;
using Shared.Bot.Keyboards;

public class StartCommandHandler(
    IParentRepository parentRepository,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ILogger<StartCommandHandler> logger) : IRequestHandler<StartCommand, Unit>
{
    public async Task<Unit> Handle(StartCommand request, CancellationToken ct)
    {
        logger.LogInformation("Processing /start command for ChatId: {ChatId}", request.ChatId);

        var existingParent = await parentRepository.GetAsync(request.ChatId, ct);

        if (existingParent != null)
        {
            await SendWelcomeBackMessage(request.ChatId, existingParent, ct);
        }
        else
        {
            var newParent = new Parent
            {
                ChatId = request.ChatId,
                Name = request.FirstName ?? request.Username ?? "Родитель"
            };

            if (!await parentRepository.CreateAsync(newParent, ct))
            {
                logger.LogInformation("Failed to create new Parent with ChatId: {ChatId}", request.ChatId);
                throw new Exception($"Failed to create parent with id: {newParent.Id}");
            }

            logger.LogInformation("Created new Parent with ChatId: {ChatId}", request.ChatId);

            await SendWelcomeMessage(request.ChatId, newParent.Name, ct);
        }
        
        await userStateService.ResetAsync(request.ChatId, ct); 
    
        logger.LogInformation("Initialized Idle state for chat {ChatId}", request.ChatId);

        return Unit.Value;
    }

    private async Task SendWelcomeMessage(long chatId, string name, CancellationToken ct)
    {
        var welcomeText = $"""
                           👋 Привет, {name}!

                           Я помогу тебе сохранить важные моменты из жизни твоих детей! 📸

                           🎯 Что я умею:
                           • Записывать воспоминания с фото и видео
                           • Отслеживать важные вехи развития
                           • Создавать хронологию событий

                           Для начала добавь своего ребёнка:
                           """;

        await messageService.SendTextMessageAsync(chatId, welcomeText, BotKeyboards.WelcomeKeyboard, ct);
    }

    private async Task SendWelcomeBackMessage(long chatId, Parent parent, CancellationToken ct)
    {
        var childrenCount = parent.Children.Count;
        var welcomeBackText = $"""
                               С возвращением, {parent.Name}! 👋

                               У тебя {childrenCount} {GetChildrenWord(childrenCount)} в системе.

                               Что будем делать?
                               """;

        await messageService.SendTextMessageAsync(chatId, welcomeBackText, BotKeyboards.MainMenuKeyboard, ct);
    }

    private static string GetChildrenWord(int count)
    {
        return count switch
        {
            1 => "ребёнок",
            2 or 3 or 4 => "ребёнка",
            _ => "детей"
        };
    }
}