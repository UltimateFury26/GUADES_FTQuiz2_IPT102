namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Hobbies.</summary>
    public class FTQuiz2_Hobby
    {
        public int HobbyId { get; set; }             // INT IDENTITY PK
        public string Title { get; set; }            // NVARCHAR(100) NOT NULL
        public string Description { get; set; }      // NVARCHAR(500) NULL
        public string ImagePath { get; set; }        // NVARCHAR(260) NULL
        public int SortOrder { get; set; }           // INT NOT NULL, default 0
    }
}
