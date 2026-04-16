namespace MilestoneTracker.Application.Common.Constants;

public static class UiConstants
{
    public static class ReplyButtons
    {
        public const string AddChild = "➕ Добавить ребёнка";
        public const string AddMilestone = "➕ Добавить воспоминание";
        public const string MyChildren = "👶 Мои дети";
        public const string History = "📜 История";
        public const string Help = "📋 Помощь";
    }
    
    public static class SlashCommands
    {
        public const string Start = "start";
        public const string Help = "help";
        public const string Cancel = "cancel";
    }

    public static class CallbackQueries
    {
        public const string Date = "date";
        public const string Next = "next";
        public const string Previous = "prev";
        
    }
}