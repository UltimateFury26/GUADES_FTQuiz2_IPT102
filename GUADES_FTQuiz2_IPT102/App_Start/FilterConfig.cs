using System.Web.Mvc;

namespace GUADES_FTQuiz2_IPT102
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new FTQuiz2_Security.FTQuiz2_NoticeFilter());
        }
    }
}
