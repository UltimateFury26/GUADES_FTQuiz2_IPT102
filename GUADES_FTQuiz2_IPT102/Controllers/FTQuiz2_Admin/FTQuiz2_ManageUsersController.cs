using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Port of Admin/ManageUsers.aspx / .cs: list, sort, deactivate, reactivate and delete users.
    /// FTQuiz2_AdminController is the AdminPage check (anonymous -> Login, non-Admin ->
    /// Home?notice=noaccess, deactivated -> signed out) and sets the FTQuiz2_admin body class (D-11..D-16).
    /// The stored procedures repeat the rules (no self-action, no action on an Admin), so the
    /// disabled buttons in the view are a convenience, not the only protection.
    /// Search and the status filter stay client-side (FTQuiz2_Portfolio.js), so every user is always loaded.
    /// </summary>
    public class FTQuiz2_ManageUsersController : FTQuiz2_AdminController
    {
        private const string MessageKey = "FTQuiz2_ManageUsers_Message";
        private const string MessageKindKey = "FTQuiz2_ManageUsers_MessageKind";

        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";
        private const string UserNotFoundMessage = "That user no longer exists.";
        private const string SelfActionMessage = "You can't change or delete your own account here.";
        private const string AdminTargetMessage = "Admin accounts can't be changed or deleted.";
        private const string NotAllowedMessage = "You don't have permission to do that.";
        private const string GenericFailureMessage = "That action couldn't be completed.";

        // The only sortable columns (the header buttons of the source Repeater).
        private static readonly string[] SortFields =
            { "UserId", "Username", "Email", "Phone", "Role", "Status", "CreatedAt", "LastLoginAt" };

        // Source: header click -> postback with ViewState sort. Here: header link -> ?sort=&dir=.
        [HttpGet]
        public ActionResult Index(string sort, string dir)
        {
            var model = new FTQuiz2_ManageUsersViewModel
            {
                CurrentUserId = CurrentUserId,
                SortField = SortFields.Contains(sort) ? sort : null,
                SortAsc = !string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase)
            };

            // Outcome of the last row action (post/redirect/get).
            string message = TempData[MessageKey] as string;
            if (message != null)
            {
                model.Message = message;
                model.MessageKind = (TempData[MessageKindKey] as string) ?? "error";
            }

            try
            {
                model.Users = new FTQuiz2_UserRepository().GetUsers(null, null);
                model.Loaded = true;
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                model.Message = DatabaseMessage;
                model.MessageKind = "error";
                model.Loaded = false;
                return View(model);
            }

            SortUsers(model.Users, model.SortField, model.SortAsc);
            return View(model);
        }

        // Source rptUsers_ItemCommand "deactivate" / "reactivate" (status = the status to set).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetStatus(int userId, string status, string sort, string dir)
        {
            int code;
            string name;

            try
            {
                var repo = new FTQuiz2_UserRepository();
                FTQuiz2_User target = repo.GetById(userId);
                name = target == null ? "That user" : target.Username;
                code = repo.SetUserStatus(CurrentUserId, userId, status);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Done(DatabaseMessage, false, sort, dir);
            }

            if (code == FTQuiz2_Constants.AdminActionCodes.Ok)
            {
                string text = status == FTQuiz2_Constants.Status.Inactive
                    ? name + " was deactivated. They can no longer log in."
                    : name + " was reactivated.";
                return Done(text, true, sort, dir);
            }

            return Done(FailureText(code), false, sort, dir);
        }

        // Source rptUsers_ItemCommand "delete".
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int userId, string sort, string dir)
        {
            int code;
            string name;

            try
            {
                var repo = new FTQuiz2_UserRepository();
                FTQuiz2_User target = repo.GetById(userId);
                name = target == null ? "That user" : target.Username;
                code = repo.DeleteUser(CurrentUserId, userId);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Done(DatabaseMessage, false, sort, dir);
            }

            return code == FTQuiz2_Constants.AdminActionCodes.Ok
                ? Done(name + " was deleted.", true, sort, dir)
                : Done(FailureText(code), false, sort, dir);
        }

        // Keep the admin's current sort across the redirect (the source kept it in ViewState).
        private ActionResult Done(string text, bool success, string sort, string dir)
        {
            TempData[MessageKey] = text;
            TempData[MessageKindKey] = success ? "success" : "error";
            return RedirectToAction("Index", new { sort = sort, dir = dir });
        }

        private static string FailureText(int code)
        {
            switch (code)
            {
                case FTQuiz2_Constants.AdminActionCodes.UserNotFound: return UserNotFoundMessage;
                case FTQuiz2_Constants.AdminActionCodes.CannotActOnSelf: return SelfActionMessage;
                case FTQuiz2_Constants.AdminActionCodes.CannotActOnAdmin: return AdminTargetMessage;
                case FTQuiz2_Constants.AdminActionCodes.NotAnAdmin: return NotAllowedMessage;
                default: return GenericFailureMessage;
            }
        }

        // Source SortUsers: sort ascending by the chosen column, then reverse for descending.
        // OrderBy is stable, so equal values keep the procedure's newest-first order.
        private static void SortUsers(List<FTQuiz2_User> users, string field, bool asc)
        {
            if (string.IsNullOrEmpty(field)) { return; }

            IEnumerable<FTQuiz2_User> sorted;

            switch (field)
            {
                case "Username":
                    sorted = users.OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase); break;
                case "Email":
                    sorted = users.OrderBy(u => u.Email, StringComparer.OrdinalIgnoreCase); break;
                case "Phone":
                    sorted = users.OrderBy(u => u.Phone, StringComparer.Ordinal); break;
                case "Role":
                    sorted = users.OrderBy(u => u.Role, StringComparer.Ordinal); break;
                case "Status":
                    sorted = users.OrderBy(u => u.Status, StringComparer.Ordinal); break;
                case "CreatedAt":
                    sorted = users.OrderBy(u => u.CreatedAt); break;
                case "LastLoginAt":
                    sorted = users.OrderBy(u => u.LastLoginAt); break;   // never-logged-in (null) first
                default:
                    sorted = users.OrderBy(u => u.UserId); break;
            }

            List<FTQuiz2_User> result = sorted.ToList();
            if (!asc) { result.Reverse(); }

            users.Clear();
            users.AddRange(result);
        }

        // Source caught SqlException. EF can wrap it (e.g. when the connection cannot open).
        private static bool IsDatabaseError(Exception ex)
        {
            for (Exception e = ex; e != null; e = e.InnerException)
            {
                if (e is SqlException) { return true; }
            }
            return false;
        }
    }
}
