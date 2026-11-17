namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>
    /// Central place for fixed strings and result codes (port of source Helpers/Constants.cs).
    /// Values match the CHECK constraints and result codes in GUADES_QuizScript_IPT102.sql.
    /// The connection name lives in FTQuiz2_AppSettings.ConnectionName instead.
    /// </summary>
    public static class FTQuiz2_Constants
    {
        /// <summary>User roles (Users.Role).</summary>
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
        }

        /// <summary>Account statuses (Users.Status).</summary>
        public static class Status
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
        }

        /// <summary>Skill categories (Skills.Category); each one becomes a board.</summary>
        public static class SkillCategories
        {
            public const string Frontend = "Frontend";
            public const string Backend = "Backend";
            public const string Tools = "Tools";
        }

        /// <summary>Query-string keys used for one-time notices between pages.</summary>
        public static class Notices
        {
            public const string Key = "notice";
            public const string NoAccess = "noaccess";
            public const string SignedUp = "signedup";
            public const string Deactivated = "deactivated";
        }

        /// <summary>Result codes returned by usp_User_Login.</summary>
        public static class LoginCodes
        {
            public const int Ok = 0;
            public const int InvalidCredentials = 1;
            public const int Inactive = 2;
        }

        /// <summary>Result codes returned by usp_User_Register.</summary>
        public static class RegisterCodes
        {
            public const int Ok = 0;
            public const int UsernameTaken = 1;
            public const int EmailTaken = 2;
            public const int InvalidInput = 3;
        }

        /// <summary>Result codes returned by usp_User_UpdateAccount.</summary>
        public static class AccountUpdateCodes
        {
            public const int Ok = 0;
            public const int EmailTaken = 1;
            public const int UserNotFound = 2;
            public const int InvalidInput = 3;
        }

        /// <summary>Result codes returned by usp_User_ChangePassword.</summary>
        public static class ChangePasswordCodes
        {
            public const int Ok = 0;
            public const int WrongCurrentPassword = 1;
            public const int UserNotFound = 2;
            public const int WeakNewPassword = 3;
        }

        /// <summary>Result codes returned by usp_Admin_SetUserStatus and usp_Admin_DeleteUser.</summary>
        public static class AdminActionCodes
        {
            public const int Ok = 0;
            public const int UserNotFound = 1;
            public const int CannotActOnSelf = 2;
            public const int CannotActOnAdmin = 3;
            public const int InvalidStatus = 4;
            public const int NotAnAdmin = 5;
        }

        /// <summary>Query-string keys used by the Manage Content page.</summary>
        public static class ContentQuery
        {
            public const string Tab = "tab";
            public const string Saved = "saved";
        }

        /// <summary>Keys of the Manage Content tabs (one per website-content section).</summary>
        public static class ContentTabs
        {
            public const string Personal = "personal";
            public const string Education = "education";
            public const string Skills = "skills";
            public const string Hobbies = "hobbies";
            public const string Projects = "projects";
            public const string Gallery = "gallery";
        }

        /// <summary>Query-string key and result codes used by the Manage Posts page after a change.</summary>
        public static class PostQuery
        {
            public const string Done = "done";
            public const string Created = "created";
            public const string Updated = "updated";
            public const string Published = "published";
            public const string Unpublished = "unpublished";
            public const string Deleted = "deleted";
        }

        /// <summary>MVC-only: TempData keys used by the notice filter and base controller (no Web Forms equivalent).</summary>
        public static class TempDataKeys
        {
            public const string Notice = "FTQuiz2_Notice";
            public const string Success = "FTQuiz2_Success";
            public const string Error = "FTQuiz2_Error";
        }
    }
}
