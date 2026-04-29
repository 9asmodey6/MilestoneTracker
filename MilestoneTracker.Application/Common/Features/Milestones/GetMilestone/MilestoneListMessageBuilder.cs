namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone;

using System.Text;
using Domain.Entities.Milestones;
using Domain.Enums;
using Models;

public static class MilestoneListMessageBuilder
{
    private static readonly string[] Emojis = { "1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣" };

    /// <summary>
    /// Формирует HTML-текст пагинированного списка воспоминаний.
    /// </summary>
    public static string BuildListMessage(
        GetMilestoneData data,
        List<Milestone> items,
        int currentPage,
        int totalPages)
    {
        var childName = data.ChildName ?? "ребёнка";
        var sb = new StringBuilder();
        
        var header = data.Mode switch
        {
            ViewMode.Category when data.SelectedCategory.HasValue =>
                $"🗂 <b>Воспоминания {childName}</b> в категории {GetCategoryName(data.SelectedCategory.Value)}",
            ViewMode.Date when data.SelectedDate.HasValue =>
                $"📆 <b>Воспоминания {childName}</b> за {data.SelectedDate.Value:dd.MM.yyyy}",
            _ =>
                $"📖 <b>Воспоминания {childName}</b>"
        };

        sb.AppendLine($"{header} (стр. {currentPage}/{totalPages}):\n");

        if (items.Count == 0)
        {
            var emptyText = data.Mode switch
            {
                ViewMode.Category when data.SelectedCategory.HasValue =>
                    $"<i>В категории {GetCategoryName(data.SelectedCategory.Value)} пока нет воспоминаний.</i>",
                ViewMode.Date when data.SelectedDate.HasValue =>
                    $"<i>За {data.SelectedDate.Value:dd.MM.yyyy} воспоминаний не найдено.</i>",
                _ =>
                    "<i>Воспоминаний пока нет.</i>"
            };
            sb.AppendLine(emptyText);
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var emoji = i < Emojis.Length ? Emojis[i] : $"{i + 1}.";
                sb.AppendLine($"{emoji} <b>{item.Title}</b> (<i>{item.OccurredAt:dd.MM.yyyy}</i>)");
            }
            sb.AppendLine("\n<i>Выбери номер воспоминания, чтобы посмотреть его:</i>");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Рассчитывает количество страниц.
    /// </summary>
    public static int CalculateTotalPages(int totalCount, int pageSize = 5)
        => Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

    public static string GetCategoryName(MilestoneCategory category) => category switch
    {
        MilestoneCategory.General => "🌐 Общее",
        MilestoneCategory.FirstTime => "🆕 Впервые",
        MilestoneCategory.Health => "🏥 Здоровье",
        MilestoneCategory.Funny => "😂 Смешное",
        MilestoneCategory.Achievement => "🏆 Достижение",
        MilestoneCategory.Adventure => "🚀 Приключение",
        MilestoneCategory.Social => "🤝 Социальное",
        MilestoneCategory.Development => "🧠 Развитие",
        MilestoneCategory.Food => "🍏 Еда",
        MilestoneCategory.Sleep => "😴 Сон",
        MilestoneCategory.Emotions => "🎭 Эмоции",
        _ => category.ToString()
    };
}
