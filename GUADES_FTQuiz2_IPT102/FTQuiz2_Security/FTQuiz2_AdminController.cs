using System.Web.Mvc;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>Every action requires an Active Admin account (AdminPage equivalent).</summary>
    [FTQuiz2_RequireAdmin]
    [FTQuiz2_RequireLogin]
    public abstract class FTQuiz2_AdminController : FTQuiz2_BaseController
    {
        // Turns on the body.FTQuiz2_admin rules in FTQuiz2_Admin.css for every admin screen.
        // Subclasses that override this must call base.OnActionExecuting(filterContext).
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.FTQuiz2_BodyClass = "FTQuiz2_admin";
        }

        protected void Flash(string message) { TempData[FTQuiz2_Constants.TempDataKeys.Success] = message; }
        protected void FlashError(string message) { TempData[FTQuiz2_Constants.TempDataKeys.Error] = message; }
    }
}
