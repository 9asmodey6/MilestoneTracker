using MilestoneTracker.Application.Common.Shared.Models;

namespace MilestoneTracker.Application.Common.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public interface ITelegramMessageService
{
    Task SendTextMessageAsync(
        long chatId,
        string text,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default);

    Task SendMessageWithInlineKeyboardAsync(
        long chatId,
        string text,
        InlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default);

    Task EditMessageTextAsync(
        long chatId,
        int messageId,
        string newText,
        InlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default);


    Task AnswerCallbackQueryAsync(
        string callbackQueryId,
        string? text = null,
        CancellationToken ct = default);

    Task SendPhotoAsync(
        long chatId,
        string photoSource,
        string? caption = null,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default);

    Task SendVideoAsync(
        long chatId,
        string videoSource,
        string? caption = null,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default);
    
    Task SendMediaGroupAsync(
        long chatId, 
        IEnumerable<MediaItem> media, 
        CancellationToken ct = default);
}