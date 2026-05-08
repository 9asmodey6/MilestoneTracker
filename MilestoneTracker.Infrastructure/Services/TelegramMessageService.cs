namespace MilestoneTracker.Infrastructure.Services;

using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MilestoneTracker.Application.Common.Shared.Models;

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
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: ct);

            logger.LogInformation("Sent message to {ChatId}", chatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to {ChatId}", chatId);
        }
    }

    public async Task SendMessageWithInlineKeyboardAsync(long chatId, string text, InlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default)
    {
        await SendTextMessageAsync(chatId, text, keyboard, ct);
    }

    public async Task EditMessageTextAsync(long chatId, int messageId, string newText,
        InlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default)
    {
        try
        {
            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: newText,
                replyMarkup: keyboard,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: ct);

            logger.LogInformation("Edited message {MessageId} for {ChatId}", messageId, chatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to edit message {MessageId}", messageId);
        }
    }

    public async Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null,
        CancellationToken ct = default)
    {
        try
        {
            await botClient.AnswerCallbackQuery(
                callbackQueryId: callbackQueryId,
                text: text,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to answer callback query {Id}", callbackQueryId);
        }
    }

    public async Task SendPhotoAsync(
        long chatId,
        string photoSource,
        string? caption = null,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        try
        {
            InputFile photo = photoSource.StartsWith("http")
                ? InputFile.FromUri(photoSource)
                : InputFile.FromFileId(photoSource);

            await botClient.SendPhoto(
                chatId: chatId,
                photo: photo,
                caption: caption,
                replyMarkup: replyMarkup,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: ct
            );

            logger.LogInformation("Sent photo to {ChatId}", chatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send photo to {ChatId}", chatId);
        }
    }

    public async Task SendVideoAsync(
        long chatId,
        string videoSource,
        string? caption = null,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        try
        {
            InputFile video = videoSource.StartsWith("http")
                ? InputFile.FromUri(videoSource)
                : InputFile.FromFileId(videoSource);

            await botClient.SendVideo(
                chatId: chatId,
                video: video,
                caption: caption,
                replyMarkup: replyMarkup,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: ct
            );

            logger.LogInformation("Sent video to {ChatId}", chatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send video to {ChatId}", chatId);
        }
    }

    public async Task SendMediaGroupAsync(
        long chatId,
        IEnumerable<MediaItem> media,
        CancellationToken ct = default)
    {
        try
        {
            var telegramMedia = media.Select(m => m.Type switch
            {
                MediaType.Photo => (IAlbumInputMedia)new InputMediaPhoto(InputFile.FromFileId(m.FileId)) { Caption = m.Caption, ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html },
                MediaType.Video => new InputMediaVideo(InputFile.FromFileId(m.FileId)) { Caption = m.Caption, ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html },
                _ => throw new ArgumentOutOfRangeException()
            });

            await botClient.SendMediaGroup(
                chatId: chatId,
                media: telegramMedia,
                cancellationToken: ct
            );

            logger.LogInformation("Sent media group to {ChatId}", chatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send media group to {ChatId}", chatId);
        }
    }
}