using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Port of MyAccount.aspx / MyAccount.aspx.cs. A signed-in user (or Admin) sees their own
    /// username, updates their own email and phone, and changes their own password.
    /// [FTQuiz2_RequireLogin] is the BasePage check (ticket + account still Active on every request).
    /// Messages, result-code handling and focus behaviour are the source's.
    /// </summary>
    [FTQuiz2_RequireLogin]
    public class FTQuiz2_MyAccountController : FTQuiz2_BaseController
    {
        private const string ErrorKind = "error";
        private const string SuccessKind = "success";

        private const string InvalidAccountInputMessage = "Something in the form isn't valid. Check each field and try again.";
        private const string InvalidPasswordInputMessage = "That new password doesn't meet the requirements.";
        private const string AccountUpdatedMessage = "Your account details were updated.";
        private const string PasswordChangedMessage = "Your password was changed.";
        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";

        // Success messages survive the redirect (post/redirect/get) in TempData, one key per card.
        private const string AccountMessageKey = "FTQuiz2_MyAccount_AccountMessage";
        private const string PasswordMessageKey = "FTQuiz2_MyAccount_PasswordMessage";

        // D-17: turns on the body.FTQuiz2_account rules in FTQuiz2_Admin.css for Index and for
        // both POST actions that redisplay the page. Set here (not in the view) because the
        // layout needs the value; this runs for all three actions.
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.FTQuiz2_BodyClass = "FTQuiz2_account";
            ViewBag.Title = "My Account";
        }

        [HttpGet]
        public ActionResult Index()
        {
            FTQuiz2_User account;

            try { account = new FTQuiz2_UserRepository().GetById(CurrentUserId); }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                ShowAccountMessage(DatabaseMessage, ErrorKind);
                return View(new FTQuiz2_MyAccountViewModel());
            }

            if (account == null) { return SignOutDeactivated(); }

            string accountMessage = TempData[AccountMessageKey] as string;
            string passwordMessage = TempData[PasswordMessageKey] as string;
            if (accountMessage != null) { ShowAccountMessage(accountMessage, SuccessKind); }
            if (passwordMessage != null) { ShowPasswordMessage(passwordMessage, SuccessKind); }

            return View(new FTQuiz2_MyAccountViewModel
            {
                Username = account.Username,
                Email = account.Email,
                Phone = account.Phone
            });
        }

        // Source btnSaveAccount_Click.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAccount(FTQuiz2_MyAccountViewModel model)
        {
            // Only the Account Information fields belong to this form.
            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");

            if (!ModelState.IsValid) { return Redisplay(model, reloadContact: false); }

            int code;

            try
            {
                code = new FTQuiz2_UserRepository().UpdateAccount(CurrentUserId, model.Email.Trim(), model.Phone.Trim());
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                ShowAccountMessage(DatabaseMessage, ErrorKind);
                return Redisplay(model, reloadContact: false);
            }

            switch (code)
            {
                case FTQuiz2_Constants.AccountUpdateCodes.Ok:
                    TempData[AccountMessageKey] = AccountUpdatedMessage;
                    return RedirectToAction("Index");

                case FTQuiz2_Constants.AccountUpdateCodes.EmailTaken:
                    ModelState.AddModelError("Email", "Another account already uses that email.");
                    ViewBag.FTQuiz2_FocusId = "Email";
                    break;

                case FTQuiz2_Constants.AccountUpdateCodes.UserNotFound:
                    return SignOutDeactivated();

                default:
                    ShowAccountMessage(InvalidAccountInputMessage, ErrorKind);
                    break;
            }

            return Redisplay(model, reloadContact: false);
        }

        // Source btnChangePassword_Click.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(FTQuiz2_MyAccountViewModel model)
        {
            // Only the Change Password fields belong to this form.
            ModelState.Remove("Email");
            ModelState.Remove("Phone");

            if (!ModelState.IsValid) { return Redisplay(model, reloadContact: true); }

            int code;

            try
            {
                code = new FTQuiz2_UserRepository().ChangePassword(CurrentUserId, model.CurrentPassword, model.NewPassword);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                ShowPasswordMessage(DatabaseMessage, ErrorKind);
                return Redisplay(model, reloadContact: true);
            }

            switch (code)
            {
                case FTQuiz2_Constants.ChangePasswordCodes.Ok:
                    // Password fields are never re-rendered, so the redirect leaves them empty (source cleared them).
                    TempData[PasswordMessageKey] = PasswordChangedMessage;
                    return RedirectToAction("Index");

                case FTQuiz2_Constants.ChangePasswordCodes.WrongCurrentPassword:
                    ModelState.AddModelError("CurrentPassword", "That current password is incorrect.");
                    ViewBag.FTQuiz2_FocusId = "CurrentPassword";
                    break;

                case FTQuiz2_Constants.ChangePasswordCodes.UserNotFound:
                    return SignOutDeactivated();

                default: // WeakNewPassword and anything unexpected
                    ShowPasswordMessage(InvalidPasswordInputMessage, ErrorKind);
                    break;
            }

            return Redisplay(model, reloadContact: true);
        }

        // Username is read-only and never posted, so it is reloaded from the database. When the
        // password form was the one posted, email and phone were not posted either, so they are
        // reloaded too (after the ModelState keys were removed, TextBoxFor then shows these values).
        private ActionResult Redisplay(FTQuiz2_MyAccountViewModel model, bool reloadContact)
        {
            FTQuiz2_User account;

            try { account = new FTQuiz2_UserRepository().GetById(CurrentUserId); }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                ShowAccountMessage(DatabaseMessage, ErrorKind);
                return View("Index", model);
            }

            if (account == null) { return SignOutDeactivated(); }

            model.Username = account.Username;
            if (reloadContact)
            {
                model.Email = account.Email;
                model.Phone = account.Phone;
            }

            return View("Index", model);
        }

        // Source SignOutWithNotice(Notices.Deactivated): sign out, go to Login?notice=deactivated.
        private ActionResult SignOutDeactivated()
        {
            FTQuiz2_FormsAuthService.SignOut(HttpContext);
            return Redirect(Url.Content("~/FTQuiz2_Login") + "?" + FTQuiz2_Constants.Notices.Key
                + "=" + Server.UrlEncode(FTQuiz2_Constants.Notices.Deactivated));
        }

        private void ShowAccountMessage(string text, string kind)
        {
            ViewBag.FTQuiz2_AccountMessage = text;
            ViewBag.FTQuiz2_AccountMessageKind = kind;
        }

        private void ShowPasswordMessage(string text, string kind)
        {
            ViewBag.FTQuiz2_PasswordMessage = text;
            ViewBag.FTQuiz2_PasswordMessageKind = kind;
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
