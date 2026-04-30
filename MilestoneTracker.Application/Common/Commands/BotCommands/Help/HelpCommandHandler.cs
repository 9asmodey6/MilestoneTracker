namespace MilestoneTracker.Application.Common.Commands.BotCommands.Help;

using MediatR;
using Microsoft.Extensions.Logging;
using MilestoneTracker.Application.Common.Interfaces;
using MilestoneTracker.Application.Common.Shared.Bot.Keyboards;

public class HelpCommandHandler(
    ITelegramMessageService messageService,
    ILogger<HelpCommandHandler> logger) : IRequestHandler<HelpCommand, Unit>
{
    public async Task<Unit> Handle(HelpCommand request, CancellationToken ct)
    {
        logger.LogInformation("Processing Help command for ChatId: {ChatId}", request.ChatId);

        var helpText = """
            <b>📋 Справка по использованию бота</b>

            Я помогу тебе сохранить самые важные моменты взросления твоих детей! 📸

            <b>🎯 Основные функции:</b>
            • <b>➕ Добавить ребёнка</b> — создай профиль для своего малыша, чтобы начать записывать его успехи.
            • <b>👶 Мои дети</b> — список всех твоих детей в системе.
            • <b>➕ Добавить воспоминание</b> — сохрани важное событие. Можно прикрепить фото или видео!
            • <b>📜 Воспоминания</b> — удобный просмотр истории событий по датам или категориям.
            • <b>🍼 Поделиться доступом</b> — создай специальный код, чтобы второй родитель тоже мог видеть и добавлять воспоминания.
            • <b>🔑 Добавить по коду</b> — если тебе прислали код доступа, введи его здесь.

            <b>💡 Советы:</b>
            • Если ты находишься в процессе заполнения данных и хочешь прерваться — используй кнопку или команду /cancel.
            • При добавлении воспоминания старайся выбирать подходящую категорию, чтобы потом было легче искать.

            Если у тебя остались вопросы, просто начни пользоваться ботом, он подскажет следующий шаг! 😊
            """;

        await messageService.SendTextMessageAsync(
            request.ChatId, 
            helpText, 
            BotKeyboards.MainMenuKeyboard, 
            ct);

        return Unit.Value;
    }
}
