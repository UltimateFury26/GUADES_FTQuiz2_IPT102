namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<FTQuiz2_Data.FTQuiz2_PortfolioDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "GUADES_FTQuiz2_IPT102.FTQuiz2_Data.FTQuiz2_PortfolioDbContext";
            MigrationsDirectory = "FTQuiz2_Migrations";
        }

        protected override void Seed(FTQuiz2_Data.FTQuiz2_PortfolioDbContext context)
        {
            // Intentionally empty: data is seeded by GUADES_QuizScript_IPT102.sql (no duplicate seeding).
        }
    }
}
