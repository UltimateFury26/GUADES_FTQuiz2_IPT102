using System;
using System.Collections.Generic;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Users.</summary>
    public class FTQuiz2_User
    {
        public int UserId { get; set; }              // INT IDENTITY PK
        public string Username { get; set; }         // NVARCHAR(50) NOT NULL, UNIQUE
        public string Email { get; set; }            // NVARCHAR(100) NOT NULL, UNIQUE
        public string Phone { get; set; }            // VARCHAR(11) NOT NULL

        // Never written through EF (ADR-002): passwords are hashed inside the stored procedures.
        public byte[] PasswordHash { get; set; }     // VARBINARY(32) NOT NULL
        public Guid PasswordSalt { get; set; }       // UNIQUEIDENTIFIER NOT NULL

        public string Role { get; set; }             // VARCHAR(10) NOT NULL, default 'User'
        public string Status { get; set; }           // VARCHAR(10) NOT NULL, default 'Active'
        public DateTime CreatedAt { get; set; }      // DATETIME2(0) NOT NULL, default SYSDATETIME()
        public DateTime? LastLoginAt { get; set; }   // DATETIME2(0) NULL

        // Inverse ends of FK_Posts_Author and FK_PageVisits_User (the only two relationships).
        public virtual ICollection<FTQuiz2_Post> Posts { get; set; }
        public virtual ICollection<FTQuiz2_PageVisit> Visits { get; set; }

        public FTQuiz2_User()
        {
            Posts = new HashSet<FTQuiz2_Post>();
            Visits = new HashSet<FTQuiz2_PageVisit>();
        }
    }
}
