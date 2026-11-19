using System.Collections.Generic;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Owners / Company information page (route /Owners). The records come from SQL Server
    /// (usp_Owner_GetAll). A record still holding the seed's "to be completed" text is shown with a visible
    /// placeholder label; nothing is invented. Same login rule as the homepage.
    /// </summary>
    [FTQuiz2_RequireLogin]
    public class FTQuiz2_BusinessController : FTQuiz2_BaseController
    {
        public ActionResult Owners()
        {
            var model = new FTQuiz2_OwnersViewModel
            {
                BusinessName = FTQuiz2_ChromeViewModel.ForRequest().OwnerName
            };

            List<FTQuiz2_OwnerInfo> owners = new FTQuiz2_BusinessRepository().GetOwners();

            foreach (FTQuiz2_OwnerInfo owner in owners)
            {
                model.Owners.Add(new FTQuiz2_OwnerCardView
                {
                    OwnerId = owner.OwnerId,
                    FullName = owner.FullName,
                    Position = owner.Position ?? string.Empty,
                    Bio = owner.Bio ?? string.Empty,
                    Email = FTQuiz2_BusinessFormat.SafeEmail(owner.Email),
                    PhotoUrl = FTQuiz2_ImagePath.Resolve(owner.PhotoPath),
                    IsPlaceholder = FTQuiz2_BusinessFormat.IsPlaceholder(owner.FullName)
                        || FTQuiz2_BusinessFormat.IsPlaceholder(owner.Bio)
                });
            }

            ViewBag.Title = "Owners / Company";
            return View(model);
        }
    }
}
