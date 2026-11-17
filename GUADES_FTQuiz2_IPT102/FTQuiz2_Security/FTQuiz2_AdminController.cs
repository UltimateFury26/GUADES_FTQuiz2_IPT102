namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>Every action requires an Active Admin account (AdminPage equivalent).</summary>
    [FTQuiz2_RequireAdmin]
    [FTQuiz2_RequireLogin]
    public abstract class FTQuiz2_AdminController : FTQuiz2_BaseController
    {
        protected void Flash(string message) { TempData[FTQuiz2_Constants.TempDataKeys.Success] = message; }
        protected void FlashError(string message) { TempData[FTQuiz2_Constants.TempDataKeys.Error] = message; }
    }
}
