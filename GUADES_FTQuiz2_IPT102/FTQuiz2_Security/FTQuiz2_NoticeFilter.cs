using System.Web.Mvc;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>
    /// Turns ?notice=noaccess|signedup|deactivated and TempData success/error into
    /// ViewBag.FTQuiz2_NoticeText / FTQuiz2_NoticeKind (error | success | info), so views never
    /// read the query string. Wording and kinds match the Web Forms source:
    /// noaccess (Site.Master, alert-error), signedup (Login, alert-success),
    /// deactivated (Login, alert-info).
    /// </summary>
    public class FTQuiz2_NoticeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Controller controller = filterContext.Controller as Controller;
            if (controller == null) { return; }

            string notice = filterContext.HttpContext.Request.QueryString[FTQuiz2_Constants.Notices.Key];
            string text = null;
            string kind = null;

            switch (notice)
            {
                case FTQuiz2_Constants.Notices.NoAccess:
                    text = "You don't have access to that page."; kind = "error"; break;
                case FTQuiz2_Constants.Notices.SignedUp:
                    text = "Account created. Log in to continue."; kind = "success"; break;
                case FTQuiz2_Constants.Notices.Deactivated:
                    text = "You were signed out because your account is no longer active."; kind = "info"; break;
            }

            if (text != null)
            {
                controller.ViewBag.FTQuiz2_NoticeText = text;
                controller.ViewBag.FTQuiz2_NoticeKind = kind;
            }

            object success = controller.TempData[FTQuiz2_Constants.TempDataKeys.Success];
            object error = controller.TempData[FTQuiz2_Constants.TempDataKeys.Error];

            if (success != null)
            {
                controller.ViewBag.FTQuiz2_NoticeText = success;
                controller.ViewBag.FTQuiz2_NoticeKind = "success";
            }
            else if (error != null)
            {
                controller.ViewBag.FTQuiz2_NoticeText = error;
                controller.ViewBag.FTQuiz2_NoticeKind = "error";
            }
        }
    }
}
