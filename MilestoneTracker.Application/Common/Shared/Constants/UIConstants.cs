namespace MilestoneTracker.Application.Common.Constants;

public static class UiConstants
{
    public static class ReplyButtons
    {
        public const string AddChild = "➕ Добавить ребёнка";
        public const string MyChildren = "👶 Мои дети";

        public const string SelectMilestoneAction = "📜 Воспоминания";
        public const string AddMilestone = "➕ Добавить воспоминание";
        public const string ViewMilestones = "📋 Посмотреть воспоминания";
        public const string RecoverMilestone = "♻️ Восстановить восспоминание";

        public const string Help = "📋 Помощь";

        public const string ProvideAccessByToken = "🍼 Поделиться доступом";
        public const string GainAccessByToken = "🔑 Добавить по коду";
    }

    public static class SlashCommandsву
    {
        public const string Start = "start";
        public const string Help = "help";
        public const string Cancel = "cancel";
    }

    public static class CallbackQueries
    {
        public const string AddChild = "add_child";
        public const string Skip = "skip";
        public const string FinishMediaUpload = "finish_media_upload";
        public const string ActionViewMilestones = "action_view_milestones";
        public const string ActionRecoverMilestones = "action_recover_milestones";

        public static class GetChild
        {
            public static string GetChildPrefix = "get_child_";
            public static string GetChildBackToList = "get_child_back_to_list";
        }

        public static class DeleteChild
        {
            public static string  DeleteChildPrefix = "delete_child_";
            public static string  DeleteChildConfirmed = "delete_child_confirmed";
        }
        
        public static class GetMilestones
        {
            public const string GetMilestonesCommand = "get_milestones";

            public const string ModeLatest = "vm_mode_latest";
            public const string SelectChildPrefix = "select_child_";
            public const string ModeCategory = "vm_mode_cat";
            public const string ModeDate = "vm_mode_date";
            public const string BackToList = "vm_back_list";
            public const string PagePrefix = "vm_page_";
            public const string ItemPrefix = "vm_item_";
        }

        public static class DeleteMilestone
        {
            public const string DeleteMilestoneCommand = "delete_milestone_";
            public const string ConfirmDeletePrefix = "confirm_del_";
            public const string AbortDelete = "deletion_aborted";
            public const string RestoreCommand = "restore_milestone";
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

        public static class RecoverMilestone
        {
            public const string PagePrefix = "rec_page_";
            public const string ItemPrefix = "rec_item_";
            public const string Confirm = "rec_confirm";
        }
    }
}