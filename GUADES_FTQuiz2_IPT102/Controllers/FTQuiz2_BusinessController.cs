using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// The old Owners / Company and Contact pages are now one page: Home/AboutUs.
    /// These actions exist only so old links and bookmarks keep working; they redirect.
    /// </summary>
    [FTQuiz2_RequireLogin]
    public class FTQuiz2_BusinessController : FTQuiz2_BaseController
    {
        public ActionResult Owners()
        {
            return RedirectToAction("AboutUs", "Home");
        }

        public ActionResult Contact()
        {
            return RedirectToAction("AboutUs", "Home");
        }
    }
}
