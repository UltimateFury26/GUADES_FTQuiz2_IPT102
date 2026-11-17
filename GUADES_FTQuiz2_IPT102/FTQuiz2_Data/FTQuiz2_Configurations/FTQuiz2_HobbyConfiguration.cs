using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Hobbies</summary>
    public class FTQuiz2_HobbyConfiguration : EntityTypeConfiguration<FTQuiz2_Hobby>
    {
        public FTQuiz2_HobbyConfiguration()
        {
            ToTable("Hobbies", "dbo");
            HasKey(h => h.HobbyId);
            Property(h => h.HobbyId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(h => h.Title).IsRequired().HasMaxLength(100);
            Property(h => h.Description).IsOptional().HasMaxLength(500);
            Property(h => h.ImagePath).IsOptional().HasMaxLength(260);
            Property(h => h.SortOrder).IsRequired();                                  // DB default 0
        }
    }
}
