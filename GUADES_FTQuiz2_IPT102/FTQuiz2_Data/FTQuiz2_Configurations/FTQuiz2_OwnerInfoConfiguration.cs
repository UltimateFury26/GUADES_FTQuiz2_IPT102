using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Owners</summary>
    public class FTQuiz2_OwnerInfoConfiguration : EntityTypeConfiguration<FTQuiz2_OwnerInfo>
    {
        public FTQuiz2_OwnerInfoConfiguration()
        {
            ToTable("Owners", "dbo");
            HasKey(o => o.OwnerId);
            Property(o => o.OwnerId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(o => o.FullName).IsRequired().HasMaxLength(100);
            Property(o => o.Position).IsOptional().HasMaxLength(100);
            Property(o => o.Bio).IsOptional().HasMaxLength(1000);
            Property(o => o.Email).IsOptional().HasMaxLength(100);
            Property(o => o.PhotoPath).IsOptional().HasMaxLength(260);
            Property(o => o.SortOrder).IsRequired();                     // DB default 0
        }
    }
}
