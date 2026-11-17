using System;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.PageVisits. FK_PageVisits_User -> Users, ON DELETE CASCADE.</summary>
    public class FTQuiz2_PageVisit
    {
        public int VisitId { get; set; }             // INT IDENTITY PK
        public int UserId { get; set; }              // INT NOT NULL, FK -> Users.UserId
        public DateTime VisitedAt { get; set; }      // DATETIME2(0) NOT NULL, default SYSDATETIME()

        public virtual FTQuiz2_User User { get; set; }
    }
}
