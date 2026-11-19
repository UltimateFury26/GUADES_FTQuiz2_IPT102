using System.Web;
using System.Web.Optimization;

namespace GUADES_FTQuiz2_IPT102
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // ---- FTQuiz2 application assets ----
            // Order is the contract: the base stylesheet first, then the Mogu Mogu brand layer (tokens and
            // shared shell, overrides the base), and the scoped admin/account guard layer last.
            bundles.Add(new StyleBundle("~/FTQuiz2_Content/css")
                        .Include("~/FTQuiz2_Content/FTQuiz2_Portfolio.css",
                                 "~/FTQuiz2_Content/FTQuiz2_MoguMogu.css"));

            bundles.Add(new StyleBundle("~/FTQuiz2_Content/admin")
                        .Include("~/FTQuiz2_Content/FTQuiz2_Admin.css"));

            bundles.Add(new ScriptBundle("~/FTQuiz2_Scripts/js")
                        .Include("~/FTQuiz2_Scripts/FTQuiz2_Portfolio.js",
                                 "~/FTQuiz2_Scripts/FTQuiz2_MoguMogu.js"));
        }
    }
}
