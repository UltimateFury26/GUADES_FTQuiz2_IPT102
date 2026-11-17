namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Skills.</summary>
    public class FTQuiz2_Skill
    {
        public int SkillId { get; set; }             // INT IDENTITY PK
        public string Name { get; set; }             // NVARCHAR(100) NOT NULL
        public string Category { get; set; }         // VARCHAR(20) NOT NULL: Frontend | Backend | Tools
        public string Description { get; set; }      // NVARCHAR(MAX) NULL
        public int SortOrder { get; set; }           // INT NOT NULL, default 0
    }
}
