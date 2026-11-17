namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Projects.</summary>
    public class FTQuiz2_Project
    {
        public int ProjectId { get; set; }           // INT IDENTITY PK
        public string Title { get; set; }            // NVARCHAR(150) NOT NULL
        public string TechStack { get; set; }        // NVARCHAR(250) NULL
        public string Role { get; set; }             // NVARCHAR(100) NULL
        public string Details { get; set; }          // NVARCHAR(MAX) NULL
        public string RepoUrl { get; set; }          // NVARCHAR(250) NULL
        public int SortOrder { get; set; }           // INT NOT NULL, default 0
    }
}
