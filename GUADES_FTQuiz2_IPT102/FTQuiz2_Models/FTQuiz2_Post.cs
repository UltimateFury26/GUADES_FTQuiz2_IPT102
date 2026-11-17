using System;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Posts. FK_Posts_Author -> Users (no cascade).</summary>
    public class FTQuiz2_Post
    {
        public int PostId { get; set; }              // INT IDENTITY PK
        public string Title { get; set; }            // NVARCHAR(150) NOT NULL
        public string Body { get; set; }             // NVARCHAR(MAX) NOT NULL
        public bool IsPublished { get; set; }        // BIT NOT NULL, default 0
        public DateTime CreatedAt { get; set; }      // DATETIME2(0) NOT NULL, default SYSDATETIME()
        public DateTime? UpdatedAt { get; set; }     // DATETIME2(0) NULL
        public int AuthorId { get; set; }            // INT NOT NULL, FK -> Users.UserId

        public virtual FTQuiz2_User Author { get; set; }
    }
}
