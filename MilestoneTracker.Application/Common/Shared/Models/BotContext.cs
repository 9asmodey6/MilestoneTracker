namespace MilestoneTracker.Infrastructure.Models;

using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

public record BotContext
{
    public long ChatId { get; init; }
    public int? MessageId { get; init; }
    public string? Text { get; init; }
    public string? CallbackData { get; init; }
    public string? CallbackQueryId { get; init; }
    public string? Payload { get; init; }

    public bool IsCallback => !string.IsNullOrEmpty(CallbackQueryId);

    public bool HasPhoto { get; init; }
    public string? PhotoFileId { get; init; }
    public bool HasVideo { get; init; }
    public string? VideoFileId { get; init; }
    public string? MediaGroupId { get; init; }
    public string? Username { get; init; }
    public string? FirstName { get; init; }

    public static BotContext? FromUpdate(Update update)
    {
        if (update.Message != null)
        {
            return new BotContext
            {
                ChatId = update.Message.Chat.Id,
                MessageId = update.Message.MessageId,
                Text = update.Message.Text,
                HasPhoto = update.Message.Photo?.Length > 0,
                PhotoFileId = update.Message.Photo?.LastOrDefault()?.FileId,
                HasVideo = update.Message.Video != null,
                VideoFileId = update.Message.Video?.FileId,
                MediaGroupId = update.Message.MediaGroupId,
                Username = update.Message.From?.Username,
                FirstName = update.Message.From?.FirstName,
                CallbackQueryId = null
            };
        }

        if (update.CallbackQuery != null)
        {
            return new BotContext
            {
                ChatId = update.CallbackQuery.Message!.Chat.Id,
                MessageId = update.CallbackQuery.Message.MessageId,
                CallbackData = update.CallbackQuery.Data,
                CallbackQueryId = update.CallbackQuery.Id,
                Username = update.CallbackQuery.From.Username,
                FirstName = update.CallbackQuery.From.FirstName,
                Text = update.CallbackQuery.Data
            };
        }

        return null;
    }
}