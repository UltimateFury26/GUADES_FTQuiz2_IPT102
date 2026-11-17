using System.Web.Mvc;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Friendly page for customErrors defaultRedirect (Error.aspx). It is public, so it must
    /// not inherit the login-required or guest base controllers. Like Error.aspx it answers 200.
    /// </summary>
    public class FTQuiz2_ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
