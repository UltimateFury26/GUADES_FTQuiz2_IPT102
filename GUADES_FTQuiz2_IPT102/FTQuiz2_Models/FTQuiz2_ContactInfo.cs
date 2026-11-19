using System;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.ContactInfo: singleton row (CK_ContactInfo_SingleRow forces ContactInfoId = 1).</summary>
    public class FTQuiz2_ContactInfo
    {
        public int ContactInfoId { get; set; }       // INT NOT NULL, default 1, PK
        public string Email { get; set; }            // NVARCHAR(100) NULL
        public string Phone { get; set; }            // VARCHAR(20) NULL
        public string Address { get; set; }          // NVARCHAR(250) NULL
        public string FacebookUrl { get; set; }      // NVARCHAR(250) NULL
        public string InstagramUrl { get; set; }     // NVARCHAR(250) NULL
        public DateTime UpdatedAt { get; set; }      // DATETIME2(0) NOT NULL, default SYSDATETIME()
    }
}
