namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Owners: owner / company representative information.</summary>
    public class FTQuiz2_OwnerInfo
    {
        public int OwnerId { get; set; }             // INT IDENTITY PK
        public string FullName { get; set; }         // NVARCHAR(100) NOT NULL
        public string Position { get; set; }         // NVARCHAR(100) NULL
        public string Bio { get; set; }              // NVARCHAR(1000) NULL
        public string Email { get; set; }            // NVARCHAR(100) NULL
        public string PhotoPath { get; set; }        // NVARCHAR(260) NULL
        public int SortOrder { get; set; }           // INT NOT NULL, default 0
    }
}
