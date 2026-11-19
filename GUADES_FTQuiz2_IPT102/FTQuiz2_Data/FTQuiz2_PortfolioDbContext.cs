using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_Configurations;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    public class FTQuiz2_PortfolioDbContext : DbContext
    {
        /// <summary>
        /// MoguMogu_DB already exists and is owned by GUADES_QuizScript_IPT102.sql.
        /// EF must never create, drop or auto-migrate it (ADR-001).
        /// </summary>
        static FTQuiz2_PortfolioDbContext()
        {
            Database.SetInitializer<FTQuiz2_PortfolioDbContext>(null);
        }

        // Same connection name FTQuiz2_AppSettings reads ("FTQuiz2_PortfolioDb" -> Initial Catalog=MoguMogu_DB).
        public FTQuiz2_PortfolioDbContext()
            : base("name=" + FTQuiz2_AppSettings.ConnectionName)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<FTQuiz2_User> Users { get; set; }
        public DbSet<FTQuiz2_PersonalInfo> PersonalInfos { get; set; }
        public DbSet<FTQuiz2_Education> Educations { get; set; }
        public DbSet<FTQuiz2_Skill> Skills { get; set; }
        public DbSet<FTQuiz2_Hobby> Hobbies { get; set; }
        public DbSet<FTQuiz2_Project> Projects { get; set; }
        public DbSet<FTQuiz2_GalleryItem> GalleryItems { get; set; }
        public DbSet<FTQuiz2_Post> Posts { get; set; }
        public DbSet<FTQuiz2_PageVisit> PageVisits { get; set; }

        // Mogu Mogu business content (Phase 2). The portfolio sets above stay until the final cleanup phase.
        public DbSet<FTQuiz2_BusinessProfile> BusinessProfiles { get; set; }
        public DbSet<FTQuiz2_ContactInfo> ContactInfos { get; set; }
        public DbSet<FTQuiz2_OwnerInfo> Owners { get; set; }
        public DbSet<FTQuiz2_Product> Products { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Every table is mapped explicitly (ToTable in each configuration).
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new FTQuiz2_UserConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_PersonalInfoConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_EducationConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_SkillConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_HobbyConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_ProjectConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_GalleryItemConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_PostConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_PageVisitConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_BusinessProfileConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_ContactInfoConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_OwnerInfoConfiguration());
            modelBuilder.Configurations.Add(new FTQuiz2_ProductConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
