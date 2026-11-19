using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Products</summary>
    public class FTQuiz2_ProductConfiguration : EntityTypeConfiguration<FTQuiz2_Product>
    {
        public FTQuiz2_ProductConfiguration()
        {
            ToTable("Products", "dbo");
            HasKey(p => p.ProductId);
            Property(p => p.ProductId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(p => p.ProductName).IsRequired().HasMaxLength(100);
            Property(p => p.Flavor).IsOptional().HasMaxLength(50);
            Property(p => p.ShortDescription).IsOptional().HasMaxLength(250);
            Property(p => p.FullDescription).IsOptional().IsMaxLength();
            Property(p => p.ImagePath).IsOptional().HasMaxLength(260);
            Property(p => p.SortOrder).IsRequired();                     // DB default 0
            Property(p => p.IsPublished).IsRequired();                   // DB default 1
            Property(p => p.CreatedAt).IsRequired().HasPrecision(0);     // DATETIME2(0), default SYSDATETIME()
            Property(p => p.UpdatedAt).IsOptional().HasPrecision(0);     // DATETIME2(0) NULL
        }
    }
}
