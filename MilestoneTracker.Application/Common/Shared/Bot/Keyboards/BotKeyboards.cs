namespace MilestoneTracker.Application.Common.Shared.Bot.Keyboards;

using MilestoneTracker.Application.Common.Constants;
using Telegram.Bot.Types.ReplyMarkups;

public static class BotKeyboards
{
    public static ReplyKeyboardMarkup WelcomeKeyboard => new([
        [new KeyboardButton(UiConstants.ReplyButtons.AddChild)],
        [new KeyboardButton(UiConstants.ReplyButtons.Help)]
    ]) { ResizeKeyboard = true };

    public static ReplyKeyboardMarkup MainMenuKeyboard => new([
        [new KeyboardButton(UiConstants.ReplyButtons.AddMilestone)],
        [
            new KeyboardButton(UiConstants.ReplyButtons.MyChildren), 
            new KeyboardButton(UiConstants.ReplyButtons.History)
        ],
        [new KeyboardButton(UiConstants.ReplyButtons.Help)]
    ]) { ResizeKeyboard = true };
    
    public static InlineKeyboardMarkup SkipPhotoKeyboard()
    {
        return new(InlineKeyboardButton
            .WithCallbackData(
                "Пропустить фото ⏭️",
                UiConstants.CallbackQueries.SkipPhoto));
    }
       
       
}