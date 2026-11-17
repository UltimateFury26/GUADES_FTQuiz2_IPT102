using System;
using System.Web.Mvc;
using System.Web.Security;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>
    /// MVC equivalent of AdminPage: anonymous goes to Login, a non-Admin goes to
    /// Home?notice=noaccess. Always pair with [FTQuiz2_RequireLogin] so a deactivated
    /// admin is still signed out.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FTQuiz2_RequireAdminAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var http = filterContext.HttpContext;

            if (!FTQuiz2_CurrentUser.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult(
                    FormsAuthentication.LoginUrl + "?ReturnUrl=" + http.Server.UrlEncode(http.Request.RawUrl));
                return;
            }

            if (!FTQuiz2_CurrentUser.IsAdmin)
            {
                filterContext.Result = new RedirectResult(
                    "~/?" + FTQuiz2_Constants.Notices.Key + "=" + FTQuiz2_Constants.Notices.NoAccess);
            }
        }
    }
}
