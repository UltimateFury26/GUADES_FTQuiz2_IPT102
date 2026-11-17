using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.PersonalInfo (singleton row)</summary>
    public class FTQuiz2_PersonalInfoConfiguration : EntityTypeConfiguration<FTQuiz2_PersonalInfo>
    {
        public FTQuiz2_PersonalInfoConfiguration()
        {
            ToTable("PersonalInfo", "dbo");
            HasKey(p => p.PersonalInfoId);

            // Not an identity: DB default is 1 and CK_PersonalInfo_SingleRow forces PersonalInfoId = 1.
            Property(p => p.PersonalInfoId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.FullName).IsRequired().HasMaxLength(100);
            Property(p => p.Tagline).IsOptional().HasMaxLength(200);
            Property(p => p.Bio).IsOptional().IsMaxLength();
            Property(p => p.Email).IsOptional().HasMaxLength(100);
            Property(p => p.Phone).IsOptional().HasMaxLength(20).IsUnicode(false);   // VARCHAR(20)
            Property(p => p.Address).IsOptional().HasMaxLength(250);
            Property(p => p.GitHubUrl).IsOptional().HasMaxLength(250);
            Property(p => p.LinkedInUrl).IsOptional().HasMaxLength(250);
            Property(p => p.AvatarPath).IsOptional().HasMaxLength(260);
            Property(p => p.UpdatedAt).IsRequired().HasPrecision(0);                 // DATETIME2(0), default SYSDATETIME()
        }
    }
}
