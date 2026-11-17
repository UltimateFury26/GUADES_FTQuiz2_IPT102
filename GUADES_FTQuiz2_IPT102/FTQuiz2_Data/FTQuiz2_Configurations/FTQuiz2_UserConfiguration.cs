using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations
{
    /// <summary>dbo.Users</summary>
    public class FTQuiz2_UserConfiguration : EntityTypeConfiguration<FTQuiz2_User>
    {
        public FTQuiz2_UserConfiguration()
        {
            ToTable("Users", "dbo");
            HasKey(u => u.UserId);
            Property(u => u.UserId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(u => u.Username).IsRequired().HasMaxLength(50).IsUnicode(true)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_Users_Username") { IsUnique = true }));

            Property(u => u.Email).IsRequired().HasMaxLength(100).IsUnicode(true)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("UQ_Users_Email") { IsUnique = true }));

            Property(u => u.Phone).IsRequired().HasMaxLength(11).IsUnicode(false);   // VARCHAR(11)

            // VARBINARY(32) (variable length, NOT binary(32)); never written through EF (ADR-002).
            Property(u => u.PasswordHash).IsRequired().HasMaxLength(32);
            Property(u => u.PasswordSalt).IsRequired();                              // UNIQUEIDENTIFIER

            // DB defaults: Role 'User', Status 'Active', CreatedAt SYSDATETIME() (set by the stored procedures).
            // CHECK constraints CK_Users_Role / _Status / _Phone live in the database; EF 6 cannot express them.
            Property(u => u.Role).IsRequired().HasMaxLength(10).IsUnicode(false);
            Property(u => u.Status).IsRequired().HasMaxLength(10).IsUnicode(false)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Users_Status")));

            Property(u => u.CreatedAt).IsRequired().HasPrecision(0)                  // DATETIME2(0)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Users_CreatedAt")));
            Property(u => u.LastLoginAt).IsOptional().HasPrecision(0);
        }
    }
}
