using System;
using System.Web.Mvc;
using System.Web.Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>
    /// MVC equivalent of BasePage.OnInit: require a ticket, then re-check on every request
    /// that the account still exists, is Active and still has the role stored in the ticket. Otherwise sign out and go to
    /// Login?notice=deactivated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FTQuiz2_RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var http = filterContext.HttpContext;

            if (!FTQuiz2_CurrentUser.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult(
                    FormsAuthentication.LoginUrl + "?ReturnUrl=" + http.Server.UrlEncode(http.Request.RawUrl));
                return;
            }

            FTQuiz2_User account = new FTQuiz2_UserRepository().GetById(FTQuiz2_CurrentUser.Id);

            if (account == null || account.Status != FTQuiz2_Constants.Status.Active)
            {
                FTQuiz2_FormsAuthService.SignOut(http);
                filterContext.Result = new RedirectResult(
                    FormsAuthentication.LoginUrl + "?" + FTQuiz2_Constants.Notices.Key + "="
                    + http.Server.UrlEncode(FTQuiz2_Constants.Notices.Deactivated));
                return;
            }

            // The ticket's role can be stale (e.g. an Admin role changed in the database after
            // sign-in). The database is the authority, so a mismatch forces a fresh sign-in
            // instead of trusting the cookie.
            if (!string.Equals(account.Role, FTQuiz2_CurrentUser.Role, StringComparison.Ordinal))
            {
                FTQuiz2_FormsAuthService.SignOut(http);
                filterContext.Result = new RedirectResult(FormsAuthentication.LoginUrl);
            }
        }
    }
}
