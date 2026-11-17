using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Skills</summary>
    public class FTQuiz2_SkillConfiguration : EntityTypeConfiguration<FTQuiz2_Skill>
    {
        public FTQuiz2_SkillConfiguration()
        {
            ToTable("Skills", "dbo");
            HasKey(s => s.SkillId);
            Property(s => s.SkillId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(s => s.Name).IsRequired().HasMaxLength(100);
            Property(s => s.Category).IsRequired().HasMaxLength(20).IsUnicode(false); // VARCHAR(20); CK_Skills_Category in DB
            Property(s => s.Description).IsOptional().IsMaxLength();
            Property(s => s.SortOrder).IsRequired();                                  // DB default 0
        }
    }
}
