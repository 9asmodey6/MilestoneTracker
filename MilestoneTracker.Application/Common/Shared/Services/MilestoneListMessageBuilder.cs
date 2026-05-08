namespace MilestoneTracker.Application.Common.Shared.Services;

using System.Text;
using Domain.Entities.Milestones;
using Domain.Enums;
using MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Models;

public static class MilestoneListMessageBuilder
{
    private static readonly string[] Emojis = { "1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣" };

    public static string BuildListMessage(
        string header,
        List<Milestone> items,
        int currentPage,
        int totalPages,
        bool showChildName = false)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{header} (стр. {currentPage}/{totalPages}):\n");
        if (items.Count == 0)
        {
            sb.AppendLine("<i>Воспоминаний не найдено.</i>");
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var emoji = i < Emojis.Length ? Emojis[i] : $"{i + 1}.";
                
                var childTag = showChildName && item.Child != null
                    ? $" [🧒 {item.Child.Name}]"
                    : "";
                sb.AppendLine($"{emoji} <b>{item.Title}</b>{childTag} (<i>{item.OccurredAt:dd.MM.yyyy}</i>)");
            }

            sb.AppendLine("\n<i>Выбери номер воспоминания, чтобы посмотреть его:</i>");
        }

        return sb.ToString();
    }
    
    // overload for filtered list
    public static string BuildListMessage(
        GetMilestoneData data,
        List<Milestone> items,
        int currentPage,
        int totalPages)
    {
        var childName = data.ChildName ?? "ребёнка";

        var header = data.Mode switch
        {
            ViewMode.Category when data.SelectedCategory.HasValue =>
                $"🗂 <b>Воспоминания {childName}</b> в категории {GetCategoryName(data.SelectedCategory.Value)}",
            ViewMode.Date when data.SelectedDate.HasValue =>
                $"📆 <b>Воспоминания {childName}</b> за {data.SelectedDate.Value:dd.MM.yyyy}",
            _ =>
                $"📖 <b>Воспоминания {childName}</b>"
        };
        return BuildListMessage(header, items, currentPage, totalPages, showChildName: false);
    }

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