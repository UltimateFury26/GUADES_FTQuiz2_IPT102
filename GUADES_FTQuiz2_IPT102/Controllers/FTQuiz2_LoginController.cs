using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Port of Login.aspx / Login.aspx.cs (a GuestPage: signed-in users are sent Home by the base class).
    /// Messages, focus behaviour and the return-URL rule are the source's.
    /// </summary>
    public class FTQuiz2_LoginController : FTQuiz2_GuestController
    {
        private const string HomePath = "~/";
        private const string ReturnUrlKey = "ReturnUrl";

        private const string InvalidCredentialsMessage = "That username, email or password isn't right. Check them and try again.";
        private const string InactiveMessage = "This account has been deactivated. Ask the site admin to turn it back on.";
        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";

        // The Log out button (Chrome) posts to /FTQuiz2_Logout, which routes here. A signed-in
        // user is exactly who calls it, so it must skip the guest "already signed in" redirect.
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (string.Equals(filterContext.ActionDescriptor.ActionName, "SignOut", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        [HttpGet]
        public ActionResult Index()
        {
            // Source ShowIncomingNotice: only "signedup" and "deactivated" are shown on Login.
            // (NoticeFilter has already filled them in; "noaccess" is not a Login notice.)
            string notice = Request.QueryString[FTQuiz2_Constants.Notices.Key];
            if (notice != FTQuiz2_Constants.Notices.SignedUp && notice != FTQuiz2_Constants.Notices.Deactivated)
            {
                ViewBag.FTQuiz2_NoticeText = null;
                ViewBag.FTQuiz2_NoticeKind = null;
            }

            ViewBag.FTQuiz2_FocusId = "Identifier";
            return View(new FTQuiz2_LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FTQuiz2_LoginViewModel model)
        {
            ClearNotice();

            if (!ModelState.IsValid) { return View(model); }

            FTQuiz2_LoginResult result;

            try
            {
                result = new FTQuiz2_UserRepository().Login(model.Identifier.Trim(), model.Password);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Fail(model, DatabaseMessage, null);
            }

            switch (result.ResultCode)
            {
                case FTQuiz2_Constants.LoginCodes.Ok:
                    FTQuiz2_FormsAuthService.SignIn(HttpContext, result.UserId.Value, result.Username, result.Role);
                    return Redirect(GetSafeReturnUrl());

                case FTQuiz2_Constants.LoginCodes.Inactive:
                    return Fail(model, InactiveMessage, "Identifier");

                default:
                    return Fail(model, InvalidCredentialsMessage, "txtPassword");
            }
        }

        // Source lnkLogout_Click: sign out, abandon the session, go to the login page.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignOut()
        {
            FTQuiz2_FormsAuthService.SignOut(HttpContext);
            return RedirectToAction("Index", "FTQuiz2_Login");
        }

        private ActionResult Fail(FTQuiz2_LoginViewModel model, string message, string focusId)
        {
            ViewBag.FTQuiz2_NoticeText = message;
            ViewBag.FTQuiz2_NoticeKind = "error";
            ViewBag.FTQuiz2_FocusId = focusId;
            return View(model);
        }

        private void ClearNotice()
        {
            ViewBag.FTQuiz2_NoticeText = null;
            ViewBag.FTQuiz2_NoticeKind = null;
        }

        // Source GetSafeReturnUrl: only a local path (starts with "/", not "//" or "/\"), and
        // never the login page itself; anything else goes Home.
        private string GetSafeReturnUrl()
        {
            string url = Request.QueryString[ReturnUrlKey];

            if (string.IsNullOrEmpty(url)) { return HomePath; }

            bool isLocalPath = url.StartsWith("/", StringComparison.Ordinal)
                && !url.StartsWith("//", StringComparison.Ordinal)
                && !url.StartsWith("/\\", StringComparison.Ordinal);

            bool isLoginPage = url.IndexOf("FTQuiz2_Login", StringComparison.OrdinalIgnoreCase) >= 0;

            return isLocalPath && !isLoginPage ? url : HomePath;
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
