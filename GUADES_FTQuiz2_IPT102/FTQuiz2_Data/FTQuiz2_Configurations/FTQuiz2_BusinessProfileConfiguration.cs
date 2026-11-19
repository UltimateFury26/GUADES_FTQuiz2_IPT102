using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.BusinessProfile (singleton row)</summary>
    public class FTQuiz2_BusinessProfileConfiguration : EntityTypeConfiguration<FTQuiz2_BusinessProfile>
    {
        public FTQuiz2_BusinessProfileConfiguration()
        {
            ToTable("BusinessProfile", "dbo");
            HasKey(p => p.BusinessProfileId);

            // Not an identity: DB default is 1 and CK_BusinessProfile_SingleRow forces BusinessProfileId = 1.
            Property(p => p.BusinessProfileId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.BusinessName).IsRequired().HasMaxLength(100);
            Property(p => p.Tagline).IsOptional().HasMaxLength(200);
            Property(p => p.BrandStory).IsOptional().IsMaxLength();
            Property(p => p.Mission).IsOptional().HasMaxLength(500);
            Property(p => p.Vision).IsOptional().HasMaxLength(500);
            Property(p => p.AboutImagePath).IsOptional().HasMaxLength(260);
            Property(p => p.HeroImagePath).IsOptional().HasMaxLength(260);
            Property(p => p.UpdatedAt).IsRequired().HasPrecision(0);     // DATETIME2(0)
        }
    }
}
