using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.ContactInfo (singleton row)</summary>
    public class FTQuiz2_ContactInfoConfiguration : EntityTypeConfiguration<FTQuiz2_ContactInfo>
    {
        public FTQuiz2_ContactInfoConfiguration()
        {
            ToTable("ContactInfo", "dbo");
            HasKey(c => c.ContactInfoId);

            // Not an identity: DB default is 1 and CK_ContactInfo_SingleRow forces ContactInfoId = 1.
            Property(c => c.ContactInfoId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(c => c.Email).IsOptional().HasMaxLength(100);
            Property(c => c.Phone).IsOptional().HasMaxLength(20).IsUnicode(false);   // VARCHAR(20)
            Property(c => c.Address).IsOptional().HasMaxLength(250);
            Property(c => c.FacebookUrl).IsOptional().HasMaxLength(250);
            Property(c => c.InstagramUrl).IsOptional().HasMaxLength(250);
            Property(c => c.UpdatedAt).IsRequired().HasPrecision(0);                 // DATETIME2(0)
        }
    }
}
