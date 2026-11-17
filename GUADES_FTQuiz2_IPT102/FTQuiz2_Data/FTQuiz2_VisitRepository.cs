using System.Data.SqlClient;
using System.Diagnostics;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    /// <summary>
    /// Visit recording with the source Home.aspx.cs error handling: a failed analytics
    /// insert must never break the page, so SqlException is swallowed (and traced).
    /// usp_Visit_Record itself only records for Role = 'User' (admins and unknown ids are ignored).
    /// </summary>
    public class FTQuiz2_VisitRepository
    {
        private readonly FTQuiz2_Db _db;

        public FTQuiz2_VisitRepository() : this(new FTQuiz2_Db()) { }
        public FTQuiz2_VisitRepository(FTQuiz2_Db db) { _db = db; }

        /// <summary>True when the call completed; false when SQL Server raised an error.</summary>
        public bool TryRecordVisit(int userId)
        {
            try
            {
                _db.Execute("usp_Visit_Record", FTQuiz2_Db.P("@UserId", userId));
                return true;
            }
            catch (SqlException ex)
            {
                Trace.TraceWarning("FTQuiz2 visit not recorded: {0}", ex.Message);
                return false;
            }
        }
    }
}
