using System.Web.Mvc;
using System.Web.Routing;

namespace GUADES_FTQuiz2_IPT102
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Original page names: Login.aspx, Signup.aspx, Error.aspx, logout link.
            routes.MapRoute("FTQuiz2_Login", "FTQuiz2_Login",
                new { controller = "FTQuiz2_Login", action = "Index" });
            routes.MapRoute("FTQuiz2_Signup", "FTQuiz2_Signup",
                new { controller = "FTQuiz2_Signup", action = "Index" });
            routes.MapRoute("FTQuiz2_Logout", "FTQuiz2_Logout",
                new { controller = "FTQuiz2_Login", action = "SignOut" });
            routes.MapRoute("FTQuiz2_Error", "FTQuiz2_Error",
                new { controller = "FTQuiz2_Error", action = "Index" });

            // Original /Admin/*.aspx folder.
            routes.MapRoute("FTQuiz2_Admin", "FTQuiz2_Admin/{controller}/{action}/{id}",
                new { controller = "FTQuiz2_ManageUsers", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute("FTQuiz2_Default", "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
