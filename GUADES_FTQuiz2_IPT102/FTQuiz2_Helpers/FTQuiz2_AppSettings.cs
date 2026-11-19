using System.Configuration;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers
{
    /// <summary>The only place that reads Web.config settings for FTQuiz2 code.</summary>
    public static class FTQuiz2_AppSettings
    {
        // Must match <connectionStrings> name; FTQuiz2_PortfolioDbContext uses the same name (the internal FTQuiz2_ prefix is kept on purpose).
        public const string ConnectionName = "FTQuiz2_PortfolioDb";

        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString; }
        }

        public static string UploadFolder
        {
            get { return ConfigurationManager.AppSettings["FTQuiz2:UploadFolder"] ?? "FTQuiz2_Uploads"; }
        }

        public static int MaxUploadBytes
        {
            get
            {
                int value;
                return int.TryParse(ConfigurationManager.AppSettings["FTQuiz2:MaxUploadBytes"], out value)
                    ? value : 5 * 1024 * 1024;
            }
        }

        public static string DefaultNotice
        {
            get { return ConfigurationManager.AppSettings["FTQuiz2:DefaultNotice"] ?? string.Empty; }
        }
    }
}
