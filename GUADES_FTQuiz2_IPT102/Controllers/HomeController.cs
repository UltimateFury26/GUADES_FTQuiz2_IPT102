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
    /// Mogu Mogu homepage. Login is required and the account is re-checked as Active on every request
    /// (FTQuiz2_RequireLogin + FTQuiz2_BaseController). All content comes from SQL Server through the
    /// repositories; the view only draws the view model.
    /// </summary>
    [FTQuiz2_RequireLogin]
    public class HomeController : FTQuiz2_BaseController
    {
        public ActionResult Index()
        {
            var model = new FTQuiz2_HomeViewModel();

            // One database context for the whole page; disposed when every section is loaded.
            using (var db = new FTQuiz2_Db())
            {
                var business = new FTQuiz2_BusinessRepository(db);

                LoadBusinessProfile(model, business.GetBusinessProfile());
                LoadContact(model, business.GetContactInfo());
                LoadFeaturedProducts(model, new FTQuiz2_ProductRepository(db));
                LoadLatestNews(model, new FTQuiz2_PostRepository(db));
            }

            RecordVisit();

            ViewBag.Title = "Home";
            return View(model);
        }

        // Only regular Users are counted, and a failure must never break the page
        // (FTQuiz2_VisitRepository already swallows SqlException).
        private void RecordVisit()
        {
            if (IsAdmin) { return; }
            new FTQuiz2_VisitRepository().TryRecordVisit(CurrentUserId);
        }

        // usp_BusinessProfile_Get. A missing row keeps the view model defaults; empty values stay empty
        // and the view hides any part with nothing to show.
        private static void LoadBusinessProfile(FTQuiz2_HomeViewModel model, FTQuiz2_BusinessProfile profile)
        {
            if (profile == null) { return; }

            model.BusinessName = profile.BusinessName;
            model.Tagline = profile.Tagline ?? string.Empty;
            model.BrandStory = profile.BrandStory ?? string.Empty;
            model.Mission = profile.Mission ?? string.Empty;
            model.Vision = profile.Vision ?? string.Empty;
            model.HeroUrl = FTQuiz2_ImagePath.Resolve(profile.HeroImagePath);
            model.AboutImageUrl = FTQuiz2_ImagePath.Resolve(profile.AboutImagePath);
        }

        // usp_ContactInfo_Get. Links are only kept when they are real http/https addresses.
        private static void LoadContact(FTQuiz2_HomeViewModel model, FTQuiz2_ContactInfo contact)
        {
            if (contact == null) { return; }

            model.Email = contact.Email ?? string.Empty;
            model.PhoneDisplay = contact.Phone ?? string.Empty;
            model.PhoneHref = FTQuiz2_HomeViewModel.ToPhoneHref(contact.Phone);
            model.Address = contact.Address ?? string.Empty;
            model.FacebookUrl = FTQuiz2_UrlGuard.SafeHttpUrl(contact.FacebookUrl);
            model.InstagramUrl = FTQuiz2_UrlGuard.SafeHttpUrl(contact.InstagramUrl);
        }

        // usp_Product_GetPublished: published products only, in display order; the first few are featured.
        private static void LoadFeaturedProducts(FTQuiz2_HomeViewModel model, FTQuiz2_ProductRepository products)
        {
            List<FTQuiz2_Product> published = products.GetPublishedProducts();
            model.PublishedProductCount = published.Count;

            foreach (FTQuiz2_Product product in published)
            {
                if (model.FeaturedProducts.Count >= FTQuiz2_HomeViewModel.FeaturedProductLimit) { break; }

                model.FeaturedProducts.Add(new FTQuiz2_ProductCardView
                {
                    ProductId = product.ProductId,
                    Name = product.ProductName,
                    Flavor = product.Flavor ?? string.Empty,
                    ShortDescription = product.ShortDescription ?? string.Empty,
                    FullDescription = product.FullDescription ?? string.Empty,
                    ImageUrl = FTQuiz2_ImagePath.Resolve(product.ImagePath)
                });
            }
        }

        // usp_Post_GetPublished: newest first, so the first one is the latest.
        private static void LoadLatestNews(FTQuiz2_HomeViewModel model, FTQuiz2_PostRepository posts)
        {
            List<FTQuiz2_PostView> published = posts.GetPublished();
            if (published.Count == 0) { return; }

            model.LatestPost = published[0];
            model.LatestPostDate = model.FormatDate(model.LatestPost.CreatedAt);
        }
    }
}
