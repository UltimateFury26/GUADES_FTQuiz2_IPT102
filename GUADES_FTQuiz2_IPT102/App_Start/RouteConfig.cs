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

            // Business pages (Phase 4). They must stay ABOVE the Admin route: that route accepts any
            // {controller}/{action}, so it would otherwise win URL generation for Url.Action(...).
            routes.MapRoute("FTQuiz2_OurStory", "OurStory",
                new { controller = "Home", action = "About" });
            routes.MapRoute("FTQuiz2_AboutUs", "AboutUs",
                new { controller = "Home", action = "AboutUs" });
            routes.MapRoute("FTQuiz2_Products", "Products",
                new { controller = "FTQuiz2_Products", action = "Index" });
            routes.MapRoute("FTQuiz2_ProductDetails", "Products/{id}",
                new { controller = "FTQuiz2_Products", action = "Details" });
            // Old /Owners and /Contact addresses now land on the merged About Us page.
            routes.MapRoute("FTQuiz2_Owners", "Owners",
                new { controller = "FTQuiz2_Business", action = "Owners" });
            routes.MapRoute("FTQuiz2_Contact", "Contact",
                new { controller = "FTQuiz2_Business", action = "Contact" });

            // Original /Admin/*.aspx folder.
            routes.MapRoute("FTQuiz2_Admin", "FTQuiz2_Admin/{controller}/{action}/{id}",
                new { controller = "FTQuiz2_ManageUsers", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute("FTQuiz2_Default", "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
