using System;
using System.Web.Mvc;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>
    /// GuestPage equivalent for Login/Signup: an already signed-in user goes to Home.
    /// A signed-in non-Admin whose ReturnUrl points into the admin area goes to
    /// Home?notice=noaccess (the original's special case). Error must NOT inherit this.
    /// </summary>
    public abstract class FTQuiz2_GuestController : FTQuiz2_BaseController
    {
        private const string ReturnUrlKey = "ReturnUrl";
        private const string AdminSegment = "FTQuiz2_Admin";

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!FTQuiz2_CurrentUser.IsAuthenticated) { return; }

            if (IsAdminUrl(filterContext.HttpContext.Request.QueryString[ReturnUrlKey]))
            {
                filterContext.Result = new RedirectResult(
                    "~/?" + FTQuiz2_Constants.Notices.Key + "=" + FTQuiz2_Constants.Notices.NoAccess);
                return;
            }

            filterContext.Result = new RedirectResult("~/");
        }

        // True when the path points into the admin area. Query ignored; as in the source,
        // the last segment is treated as the page name and skipped.
        private static bool IsAdminUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) { return false; }

            int queryStart = url.IndexOf('?');
            string path = queryStart >= 0 ? url.Substring(0, queryStart) : url;
            string[] segments = path.Split('/');

            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (string.Equals(segments[i], AdminSegment, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
            return false;
        }
    }
}
