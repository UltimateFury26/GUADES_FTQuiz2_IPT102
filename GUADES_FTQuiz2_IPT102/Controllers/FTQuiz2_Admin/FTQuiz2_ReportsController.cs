using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Admin reports: user summary cards, signups per month, active vs. inactive, product totals,
    /// signed-in-user Home visits, latest signups and the business content summary. Read-only: every number comes from a
    /// usp_Report_* procedure. FTQuiz2_AdminController is the AdminPage check (and sets the
    /// FTQuiz2_admin body class that FTQuiz2_Admin.css needs, D-15).
    /// Visit recording itself lives in HomeController.RecordVisit (Users only, failures swallowed).
    /// </summary>
    public class FTQuiz2_ReportsController : FTQuiz2_AdminController
    {
        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";

        [HttpGet]
        public ActionResult Index()
        {
            var model = new FTQuiz2_ReportsViewModel();

            try
            {
                var repo = new FTQuiz2_ReportRepository();

                model.SetSummary(repo.GetUserSummary());
                model.SetSignups(repo.GetSignupsByMonth());
                model.Recent = repo.GetRecentUsers();
                model.SetProductSummary(repo.GetProductSummary());
                model.Content = repo.GetContentSummary();
                model.SetVisitSummary(repo.GetVisitSummary());
                model.SetVisits(repo.GetVisitsByMonth());
                model.Loaded = true;
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                // Source: pnlReport hidden, error alert shown.
                model = new FTQuiz2_ReportsViewModel { Loaded = false, Message = DatabaseMessage };
            }

            ViewBag.Title = "Reports";
            return View(model);
        }

        // Source caught SqlException. EF can wrap it (e.g. when the connection cannot open).
        private static bool IsDatabaseError(Exception ex)
        {
            for (Exception e = ex; e != null; e = e.InnerException)
            {
                if (e is SqlException) { return true; }
            }
            return false;
        }
    }
}
