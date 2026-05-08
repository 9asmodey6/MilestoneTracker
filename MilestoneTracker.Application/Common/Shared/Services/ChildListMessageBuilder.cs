namespace MilestoneTracker.Application.Common.Shared.Services;

using System.Text;
using Domain.Entities;

public static class ChildListMessageBuilder
{
    private static readonly string[] Emojis = { "1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣", "7️⃣", "8️⃣", "9️⃣", "🔟" };

    public static string BuildListMessage(
        string header,
        List<Child> children)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{header}:\n");
        if (children.Count == 0)
        {
            sb.AppendLine("<i>Детей не найдено.</i>");
        }
        else
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var emoji = i < Emojis.Length ? Emojis[i] : $"{i + 1}.";
                
                sb.AppendLine($"{emoji} <b>{child.Name}</b>");
            }

            sb.AppendLine("\n<i>Выберите номер ребёнка, чтобы продолжить:</i>");
        }

        return sb.ToString();
    }
}
