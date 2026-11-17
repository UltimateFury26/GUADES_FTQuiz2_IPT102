using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Projects</summary>
    public class FTQuiz2_ProjectConfiguration : EntityTypeConfiguration<FTQuiz2_Project>
    {
        public FTQuiz2_ProjectConfiguration()
        {
            ToTable("Projects", "dbo");
            HasKey(p => p.ProjectId);
            Property(p => p.ProjectId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(p => p.Title).IsRequired().HasMaxLength(150);
            Property(p => p.TechStack).IsOptional().HasMaxLength(250);
            Property(p => p.Role).IsOptional().HasMaxLength(100);
            Property(p => p.Details).IsOptional().IsMaxLength();
            Property(p => p.RepoUrl).IsOptional().HasMaxLength(250);
            Property(p => p.SortOrder).IsRequired();                                  // DB default 0
        }
    }
}
