namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

public class TelegramMessageService(
    ITelegramBotClient botClient,
    ILogger<TelegramMessageService> logger) : ITelegramMessageService
{
    public async Task SendTextMessageAsync(
        long chatId,
        string text,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        try
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                replyMarkup: replyMarkup,
                cancellationToken: ct);
            
            logger.LogInformation("Sent message to {ChatId}", chatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to {ChatId}", chatId);
        }
    }

    public Task SendMessageWithInlineKeyboardAsync(long chatId, string text, InlineKeyboardMarkup keyboard,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task EditMessageTextAsync(long chatId, int messageId, string newText, InlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}