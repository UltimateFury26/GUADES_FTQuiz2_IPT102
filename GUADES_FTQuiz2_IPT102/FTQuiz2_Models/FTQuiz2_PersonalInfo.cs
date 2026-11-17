using System;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.PersonalInfo: singleton row (CK_PersonalInfo_SingleRow forces PersonalInfoId = 1).</summary>
    public class FTQuiz2_PersonalInfo
    {
        public int PersonalInfoId { get; set; }      // INT NOT NULL, default 1, PK
        public string FullName { get; set; }         // NVARCHAR(100) NOT NULL
        public string Tagline { get; set; }          // NVARCHAR(200) NULL
        public string Bio { get; set; }              // NVARCHAR(MAX) NULL
        public string Email { get; set; }            // NVARCHAR(100) NULL
        public string Phone { get; set; }            // VARCHAR(20) NULL
        public string Address { get; set; }          // NVARCHAR(250) NULL
        public string GitHubUrl { get; set; }        // NVARCHAR(250) NULL
        public string LinkedInUrl { get; set; }      // NVARCHAR(250) NULL
        public string AvatarPath { get; set; }       // NVARCHAR(260) NULL
        public DateTime UpdatedAt { get; set; }      // DATETIME2(0) NOT NULL, default SYSDATETIME()
    }
}
