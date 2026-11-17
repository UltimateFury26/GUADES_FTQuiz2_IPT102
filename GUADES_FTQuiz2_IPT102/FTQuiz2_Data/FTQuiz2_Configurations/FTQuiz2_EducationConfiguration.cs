using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Education</summary>
    public class FTQuiz2_EducationConfiguration : EntityTypeConfiguration<FTQuiz2_Education>
    {
        public FTQuiz2_EducationConfiguration()
        {
            ToTable("Education", "dbo");
            HasKey(e => e.EducationId);
            Property(e => e.EducationId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(e => e.SchoolName).IsRequired().HasMaxLength(150);
            Property(e => e.Level).IsRequired().HasMaxLength(50);
            Property(e => e.Program).IsOptional().HasMaxLength(150);

            // SMALLINT. StartYear was NOT NULL in CREATE TABLE but the script's Section A makes it NULL.
            Property(e => e.StartYear).IsOptional();
            Property(e => e.EndYear).IsOptional();                                   // NULL = still studying

            Property(e => e.Description).IsOptional().IsMaxLength();
            Property(e => e.PhotoPath).IsOptional().HasMaxLength(260);
            Property(e => e.SortOrder).IsRequired();                                 // DB default 0
            // CK_Education_Years (EndYear IS NULL OR EndYear >= StartYear) lives in the database.
        }
    }
}
