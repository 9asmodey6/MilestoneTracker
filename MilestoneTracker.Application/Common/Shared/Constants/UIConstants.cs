namespace MilestoneTracker.Application.Common.Constants;

public static class UiConstants
{
    public static class ReplyButtons
    {
        public const string AddChild = "➕ Добавить ребёнка";
        public const string AddMilestone = "➕ Добавить воспоминание";
        public const string MyChildren = "👶 Мои дети";
        public const string ViewMilestones = "📜 Посмотреть восспоминания";
        public const string Help = "📋 Помощь";
    }

    public static class SlashCommandsву
    {
        public const string Start = "start";
        public const string Help = "help";
        public const string Cancel = "cancel";
    }

    public static class CallbackQueries
    {
        public const string Skip = "skip";
        public const string FinishMediaUpload = "finish_media_upload";
        
        public const string AddMoreMedia = "add_more_media";

        public const string SelectChild = "select_child";

        public const string SelectCategory = "select_category";

        public static class GetMilestones
        {
            public const string GetMilestonesCommand = "get_milestones";
            
            public const string ModeLatest = "vm_mode_latest";
            public const string ModeCategory = "vm_mode_cat";
            public const string ModeDate = "vm_mode_date";
            public const string BackToList = "vm_back_list";
            public const string PagePrefix = "vm_page_";
            public const string ItemPrefix = "vm_item_";
        }

        public static class EditMilestone
        {
            public const string Confirm = "confirm_milestone";
            public const string EditChild = "edit_child";
            public const string EditCategory = "edit_category";
            public const string EditTitle = "edit_title";
            public const string EditDate = "edit_date";
            public const string EditDescription = "edit_description";
            public const string EditMedia = "edit_media";
        }
    }
}