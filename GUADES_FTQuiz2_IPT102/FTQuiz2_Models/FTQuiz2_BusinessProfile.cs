using System;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.BusinessProfile: singleton row (CK_BusinessProfile_SingleRow forces BusinessProfileId = 1).</summary>
    public class FTQuiz2_BusinessProfile
    {
        public int BusinessProfileId { get; set; }   // INT NOT NULL, default 1, PK
        public string BusinessName { get; set; }     // NVARCHAR(100) NOT NULL
        public string Tagline { get; set; }          // NVARCHAR(200) NULL
        public string BrandStory { get; set; }       // NVARCHAR(MAX) NULL
        public string Mission { get; set; }          // NVARCHAR(500) NULL
        public string Vision { get; set; }           // NVARCHAR(500) NULL
        public string AboutImagePath { get; set; }   // NVARCHAR(260) NULL
        public string HeroImagePath { get; set; }    // NVARCHAR(260) NULL
        public DateTime UpdatedAt { get; set; }      // DATETIME2(0) NOT NULL, default SYSDATETIME()
    }
}
