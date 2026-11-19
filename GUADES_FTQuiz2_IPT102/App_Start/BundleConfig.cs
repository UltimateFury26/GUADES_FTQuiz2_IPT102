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

            // NOTE: the legacy Modernizr / Bootstrap / Content\css bundles were removed along with
            // their files. No FTQuiz2 layout ever rendered them, so nothing references these paths.

            // Self-hosted brand type. Rendered before the base sheet so @font-face is
            // registered before any rule asks for the family.
            bundles.Add(new StyleBundle("~/FTQuiz2_Content/fonts")
                        .Include("~/FTQuiz2_Content/FTQuiz2_Fonts.css"));

            // ---- FTQuiz2 application assets ----
            // Order is the contract: base stylesheet, then the Mogu Mogu brand layer, then the
            // scoped admin/account guard layer, and finally the refresh design layer (rendered last).
            bundles.Add(new StyleBundle("~/FTQuiz2_Content/css")
                        .Include("~/FTQuiz2_Content/FTQuiz2_Portfolio.css",
                                 "~/FTQuiz2_Content/FTQuiz2_MoguMogu.css"));

            bundles.Add(new StyleBundle("~/FTQuiz2_Content/admin")
                        .Include("~/FTQuiz2_Content/FTQuiz2_Admin.css"));

            // Final design layer: must render AFTER the base and admin sheets so its tokens,
            // shell and admin-sidebar rules win the cascade.
            bundles.Add(new StyleBundle("~/FTQuiz2_Content/refresh")
                        .Include("~/FTQuiz2_Content/FTQuiz2_MoguRefresh.css"));

            bundles.Add(new ScriptBundle("~/FTQuiz2_Scripts/js")
                        .Include("~/FTQuiz2_Scripts/FTQuiz2_Portfolio.js",
                                 "~/FTQuiz2_Scripts/FTQuiz2_MoguMogu.js",
                                 "~/FTQuiz2_Scripts/FTQuiz2_Motion.js"));
        }
    }
}
