namespace MilestoneTracker.Infrastructure.Services;

using Telegram.Bot;
using Telegram.Bot.Types;

public class UpdateHandler
{
    private readonly ITelegramBotClient _botClient;

    public UpdateHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        try
        {
            if (update.Message is not { Text: { } messageText } message) 
            {
                Console.WriteLine($"⚠️ Update не содержит текстового сообщения. Type: {update.Type}");
                return;
            }

            Console.WriteLine($"📩 Получено: '{messageText}' от ChatId: {message.Chat.Id}");

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Влад, вебхук принял: {messageText} 🫡",
                cancellationToken: ct);
            
            Console.WriteLine("✅ Ответ отправлен!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка в HandleUpdateAsync: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }
}