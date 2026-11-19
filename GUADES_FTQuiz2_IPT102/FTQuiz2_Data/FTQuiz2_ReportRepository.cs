using System;
using System.Collections.Generic;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    public class FTQuiz2_UserSummary
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int NewThisMonth { get; set; }
    }

    /// <summary>usp_Report_ProductSummary: how many products exist and how many are visible to users.</summary>
    public class FTQuiz2_ProductSummary
    {
        public int TotalProducts { get; set; }
        public int PublishedProducts { get; set; }
        public int UnpublishedProducts { get; set; }
    }

    public class FTQuiz2_MonthlySignup
    {
        public string Label { get; set; }
        public int Signups { get; set; }
    }

    public class FTQuiz2_RecentUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FTQuiz2_ContentCount
    {
        public string Section { get; set; }
        public int Items { get; set; }
    }

    public class FTQuiz2_VisitSummary
    {
        public int TotalVisits { get; set; }
        public int UniqueVisitors { get; set; }
        public int VisitsToday { get; set; }
        public int VisitsThisMonth { get; set; }
    }

    public class FTQuiz2_MonthlyVisits
    {
        public string Label { get; set; }
        public int Visits { get; set; }
    }

    /// <summary>Report and visit gateway (8 methods). Every figure is aggregated in a usp_Report_* procedure.</summary>
    public class FTQuiz2_ReportRepository
    {
        private readonly FTQuiz2_Db _db;

        public FTQuiz2_ReportRepository() : this(new FTQuiz2_Db()) { }
        public FTQuiz2_ReportRepository(FTQuiz2_Db db) { _db = db; }

        // Source returns an all-zero summary when the procedure returns no row.
        public FTQuiz2_UserSummary GetUserSummary()
        {
            return _db.QueryFirstOrDefault<FTQuiz2_UserSummary>("usp_Report_UserSummary")
                   ?? new FTQuiz2_UserSummary();
        }

        // All-zero summary when the procedure returns no row (empty Products table still returns one row of zeros).
        public FTQuiz2_ProductSummary GetProductSummary()
        {
            return _db.QueryFirstOrDefault<FTQuiz2_ProductSummary>("usp_Report_ProductSummary")
                   ?? new FTQuiz2_ProductSummary();
        }

        public List<FTQuiz2_MonthlySignup> GetSignupsByMonth()
        {
            return _db.Query<FTQuiz2_MonthlySignup>("usp_Report_SignupsByMonth");
        }

        public List<FTQuiz2_RecentUser> GetRecentUsers()
        {
            return _db.Query<FTQuiz2_RecentUser>("usp_Report_RecentUsers");
        }

        public List<FTQuiz2_ContentCount> GetContentSummary()
        {
            return _db.Query<FTQuiz2_ContentCount>("usp_Report_ContentSummary");
        }

        // Source writer. Lets SqlException through; use FTQuiz2_VisitRepository.TryRecordVisit on public pages.
        public void RecordVisit(int userId)
        {
            _db.Execute("usp_Visit_Record", FTQuiz2_Db.P("@UserId", userId));
        }

        public FTQuiz2_VisitSummary GetVisitSummary()
        {
            return _db.QueryFirstOrDefault<FTQuiz2_VisitSummary>("usp_Report_VisitSummary")
                   ?? new FTQuiz2_VisitSummary();
        }

        public List<FTQuiz2_MonthlyVisits> GetVisitsByMonth()
        {
            return _db.Query<FTQuiz2_MonthlyVisits>("usp_Report_VisitsByMonth");
        }
    }
}
