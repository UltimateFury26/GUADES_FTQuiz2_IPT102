using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.GalleryItems</summary>
    public class FTQuiz2_GalleryItemConfiguration : EntityTypeConfiguration<FTQuiz2_GalleryItem>
    {
        public FTQuiz2_GalleryItemConfiguration()
        {
            ToTable("GalleryItems", "dbo");
            HasKey(g => g.GalleryId);
            Property(g => g.GalleryId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(g => g.ImagePath).IsRequired().HasMaxLength(260);
            Property(g => g.Caption).IsOptional().HasMaxLength(200);
            Property(g => g.SortOrder).IsRequired();                                  // DB default 0
        }
    }
}
