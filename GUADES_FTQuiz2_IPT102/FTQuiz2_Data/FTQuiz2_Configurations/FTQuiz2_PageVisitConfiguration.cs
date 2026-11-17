using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.PageVisits</summary>
    public class FTQuiz2_PageVisitConfiguration : EntityTypeConfiguration<FTQuiz2_PageVisit>
    {
        public FTQuiz2_PageVisitConfiguration()
        {
            ToTable("PageVisits", "dbo");
            HasKey(v => v.VisitId);
            Property(v => v.VisitId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(v => v.UserId).IsRequired();
            Property(v => v.VisitedAt).IsRequired().HasPrecision(0)                   // DATETIME2(0), default SYSDATETIME()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_PageVisits_VisitedAt")));

            // FK_PageVisits_User: ON DELETE CASCADE.
            HasRequired(v => v.User)
                .WithMany(u => u.Visits)
                .HasForeignKey(v => v.UserId)
                .WillCascadeOnDelete(true);
        }
    }
}
