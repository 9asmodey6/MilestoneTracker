namespace MilestoneTracker.Application.Common.Shared.Bot.Keyboards;

using Domain.Entities;
using Domain.Enums;
using Constants;
using Domain.Entities.Milestones;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Telegram.Bot.Types.ReplyMarkups;

public static class BotKeyboards
{
    public static ReplyKeyboardMarkup WelcomeKeyboard => new([
        [new KeyboardButton(UiConstants.ReplyButtons.AddChild)],
        [new KeyboardButton(UiConstants.ReplyButtons.Help)],
        [new KeyboardButton(UiConstants.ReplyButtons.GainAccessByToken)]
    ]) { ResizeKeyboard = true };

    public static ReplyKeyboardMarkup MainMenuKeyboard => new([
        [new KeyboardButton(UiConstants.ReplyButtons.AddMilestone)],
        [
            new KeyboardButton(UiConstants.ReplyButtons.MyChildren),
            new KeyboardButton(UiConstants.ReplyButtons.ViewMilestones),
        ],
        [
            new KeyboardButton(UiConstants.ReplyButtons.ProvideAccessByToken),
            new KeyboardButton(UiConstants.ReplyButtons.GainAccessByToken),
        ],
        [new KeyboardButton(UiConstants.ReplyButtons.Help)]
    ]) { ResizeKeyboard = true };

    public static InlineKeyboardMarkup SkipKeyboard()
    {
        return new(InlineKeyboardButton
            .WithCallbackData(
                "Пропустить ⏭️",
                UiConstants.CallbackQueries.Skip));
    }

    public static InlineKeyboardMarkup AddChildKeyboard => new InlineKeyboardMarkup(
        new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("➕ Добавить ребёнка", UiConstants.CallbackQueries.AddChild) }
        });

    public static InlineKeyboardMarkup CategorySelectionKeyboard()
    {
        var categoryNames = new Dictionary<MilestoneCategory, string>
        {
            { MilestoneCategory.General, "🌐 Общее" },
            { MilestoneCategory.FirstTime, "🆕 Впервые" },
            { MilestoneCategory.Health, "🏥 Здоровье" },
            { MilestoneCategory.Funny, "😂 Смешное" },
            { MilestoneCategory.Achievement, "🏆 Достижение" },
            { MilestoneCategory.Adventure, "🚀 Приключение" },
            { MilestoneCategory.Social, "🤝 Социальное" },
            { MilestoneCategory.Development, "🧠 Развитие" },
            { MilestoneCategory.Food, "🍏 Еда" },
            { MilestoneCategory.Sleep, "😴 Сон" },
            { MilestoneCategory.Emotions, "🎭 Эмоции" }
        };

        var buttons = Enum.GetValues<MilestoneCategory>()
            .Select(category => InlineKeyboardButton.WithCallbackData(
                text: categoryNames.TryGetValue(category, out var name) ? name : category.ToString(),
                callbackData: ((int)category).ToString()
            ));

        return new InlineKeyboardMarkup(buttons.Chunk(2));
    }

    public static InlineKeyboardMarkup ChildSelectionKeyboard(List<Child> children)
    {
        var buttons = children.Select(child =>
            InlineKeyboardButton.WithCallbackData(
                text: $"🧒 {child.Name}",
                callbackData: child.Id.ToString()
            ));

        return new InlineKeyboardMarkup(buttons.Chunk(1));
    }

    public static InlineKeyboardMarkup SelectCurrentDate()
    {
        string today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

        return new InlineKeyboardMarkup(
            InlineKeyboardButton.WithCallbackData("Сегодняшняя дата 📅", today)
        );
    }

    public static InlineKeyboardMarkup MediaUploadKeyboard(int currentCount)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"✅ Завершить ({currentCount} шт.)",
                    UiConstants.CallbackQueries.FinishMediaUpload)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "❌ Отменить создание воспоминания",
                    "/cancel")
            }
        });
    }

    public static InlineKeyboardMarkup FirstMediaUploadKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "⏭️ Пропустить",
                    UiConstants.CallbackQueries.Skip)
            }
        });
    }

        public static InlineKeyboardMarkup MilestoneConfirmationKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "✅ Всё верно, сохранить",
                    UiConstants.CallbackQueries.EditMilestone.Confirm)
            },

            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "👶 Ребёнок",
                    UiConstants.CallbackQueries.EditMilestone.EditChild),
                InlineKeyboardButton.WithCallbackData(
                    "📁 Категория",
                    UiConstants.CallbackQueries.EditMilestone.EditCategory),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "📅 Дата",
                    UiConstants.CallbackQueries.EditMilestone.EditDate),
                InlineKeyboardButton.WithCallbackData(
                    "📌 Заголовок",
                    UiConstants.CallbackQueries.EditMilestone.EditTitle),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "📝 Описание",
                    UiConstants.CallbackQueries.EditMilestone.EditDescription),
                InlineKeyboardButton.WithCallbackData(
                    "🖼 Медиа",
                    UiConstants.CallbackQueries.EditMilestone.EditMedia),
            },

            new[]
            {
                InlineKeyboardButton.WithCallbackData("❌ Отменить", "/cancel")
            }
        });
    }

    public static InlineKeyboardMarkup ViewMilestonesModeKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "📅 По хронологии (Последние)", 
                    UiConstants.CallbackQueries.GetMilestones.ModeLatest)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🗂 По категориям", 
                    UiConstants.CallbackQueries.GetMilestones.ModeCategory)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "📆 Конкретная дата", 
                    UiConstants.CallbackQueries.GetMilestones.ModeDate)
            }
        });
    }

    public static InlineKeyboardMarkup PaginationKeyboard(int currentPage, int totalPages, List<Milestone> itemsOnPage)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        
        var itemButtons = new List<InlineKeyboardButton>();
        for (int i = 0; i < itemsOnPage.Count; i++)
        {
            itemButtons.Add(InlineKeyboardButton.WithCallbackData(
                text: (i + 1).ToString(),
                callbackData: $"{UiConstants.CallbackQueries.GetMilestones.ItemPrefix}{itemsOnPage[i].Id}"));
        }
        
        if (itemButtons.Any())
        {
            buttons.Add(itemButtons.ToArray());
        }
        
        var navButtons = new List<InlineKeyboardButton>();
        
        if (currentPage > 1)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData(
                "⬅️ Назад", $"{UiConstants.CallbackQueries.GetMilestones.PagePrefix}{currentPage - 1}"));
        }
        
        if (currentPage < totalPages)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData(
                "Вперед ➡️", $"{UiConstants.CallbackQueries.GetMilestones.PagePrefix}{currentPage + 1}"));
        }

        if (navButtons.Any())
        {
            buttons.Add(navButtons.ToArray());
        }
        
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(
                "🔙 К выбору режима", 
                UiConstants.CallbackQueries.GetMilestones.BackToList)
        });

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup ViewMilestoneItemKeyboard(int milestoneId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🔙 Назад к списку",
                    UiConstants.CallbackQueries.GetMilestones.BackToList),
                InlineKeyboardButton.WithCallbackData(
                    "🗑️ Удалить восспоминание",
                    $"{UiConstants.CallbackQueries.DeleteMilestone.DeleteMilestoneCommand}{milestoneId.ToString()}"),
            }
        });
    }
    
    public static InlineKeyboardMarkup ConfirmChildForProvidingKeyboard(Child child)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"👶 Поделиться доступом к {child.Name}", 
                    child.Id.ToString()),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "❌ Отменить", 
                    "/cancel")
            }
        });
    }

    public static InlineKeyboardMarkup MilestoneDeleteConfirmationKeyboard(int milestoneId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🗑️ Да, удалить это воспоминание",
                    $"{UiConstants.CallbackQueries.DeleteMilestone.ConfirmDeletePrefix}{milestoneId}"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "🔙 Нет, вернуться к просмотру",
                    $"{UiConstants.CallbackQueries.GetMilestones.ItemPrefix}{milestoneId}"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "📜 Нет, назад к списку",
                    UiConstants.CallbackQueries.GetMilestones.BackToList),
            }
        });
    }
}