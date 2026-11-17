using System.Web.Mvc;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>Shared plumbing: current user for controllers and the layout.</summary>
    public abstract class FTQuiz2_BaseController : Controller
    {
        protected int CurrentUserId { get { return FTQuiz2_CurrentUser.Id; } }
        protected bool IsAdmin { get { return FTQuiz2_CurrentUser.IsAdmin; } }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.FTQuiz2_CurrentUsername = FTQuiz2_CurrentUser.Username;
            ViewBag.FTQuiz2_IsAdmin = FTQuiz2_CurrentUser.IsAdmin;
        }
    }
}
