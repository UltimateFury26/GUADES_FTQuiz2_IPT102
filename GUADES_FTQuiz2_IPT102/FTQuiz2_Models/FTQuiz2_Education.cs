namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Education.</summary>
    public class FTQuiz2_Education
    {
        public int EducationId { get; set; }         // INT IDENTITY PK
        public string SchoolName { get; set; }       // NVARCHAR(150) NOT NULL
        public string Level { get; set; }            // NVARCHAR(50) NOT NULL
        public string Program { get; set; }          // NVARCHAR(150) NULL

        // CREATE TABLE says SMALLINT NOT NULL, but the script's Section A runs
        // ALTER COLUMN StartYear SMALLINT NULL, and seed rows (elementary / junior high)
        // contain NULL. The live column is nullable, so the CLR type must be short?.
        public short? StartYear { get; set; }        // SMALLINT NULL (after Section A)
        public short? EndYear { get; set; }          // SMALLINT NULL (NULL = still studying)

        public string Description { get; set; }      // NVARCHAR(MAX) NULL
        public string PhotoPath { get; set; }        // NVARCHAR(260) NULL
        public int SortOrder { get; set; }           // INT NOT NULL, default 0
    }
}
