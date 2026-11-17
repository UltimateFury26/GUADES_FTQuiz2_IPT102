using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Posts</summary>
    public class FTQuiz2_PostConfiguration : EntityTypeConfiguration<FTQuiz2_Post>
    {
        public FTQuiz2_PostConfiguration()
        {
            ToTable("Posts", "dbo");
            HasKey(p => p.PostId);
            Property(p => p.PostId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(p => p.Title).IsRequired().HasMaxLength(150);
            Property(p => p.Body).IsRequired().IsMaxLength();
            Property(p => p.IsPublished).IsRequired();                                // BIT, DB default 0
            Property(p => p.CreatedAt).IsRequired().HasPrecision(0);                  // DATETIME2(0), default SYSDATETIME()
            Property(p => p.UpdatedAt).IsOptional().HasPrecision(0);
            Property(p => p.AuthorId).IsRequired();

            // FK_Posts_Author: no ON DELETE CASCADE.
            HasRequired(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .WillCascadeOnDelete(false);

            // IX_Posts_IsPublished_CreatedAt (IsPublished, CreatedAt DESC) already exists in the database.
            // EF 6 cannot express DESC, so it is NOT annotated here; Phase 06 recreates it with raw SQL.
        }
    }
}
