using System.Globalization;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Product catalogue for signed-in users (route /Products and /Products/{id}).
    /// Only published products are ever shown: an unpublished, deleted or invalid id gets the same
    /// friendly "not found" page with a 404 status. There is no cart or checkout, by design.
    /// Same login rule as the homepage (FTQuiz2_RequireLogin re-checks that the account is Active).
    /// </summary>
    [FTQuiz2_RequireLogin]
    public class FTQuiz2_ProductsController : FTQuiz2_BaseController
    {
        public ActionResult Index()
        {
            var model = new FTQuiz2_ProductsIndexViewModel
            {
                BusinessName = FTQuiz2_ChromeViewModel.ForRequest().OwnerName
            };

            // usp_Product_GetPublished: published only, already in display order.
            foreach (FTQuiz2_Product product in new FTQuiz2_ProductRepository().GetPublishedProducts())
            {
                model.Products.Add(FTQuiz2_BusinessFormat.ToCard(product));
            }

            ViewBag.Title = "Products";
            return View(model);
        }

        // The id arrives as text so a bad value (abc, 0, a number too large for an int) is a normal
        // "not found" page instead of a model-binding error.
        public ActionResult Details(string id)
        {
            int productId;
            if (!int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out productId) || productId <= 0)
            {
                return ProductNotFound();
            }

            var model = new FTQuiz2_ProductDetailsViewModel
            {
                BusinessName = FTQuiz2_ChromeViewModel.ForRequest().OwnerName
            };

            using (var db = new FTQuiz2_Db())
            {
                var products = new FTQuiz2_ProductRepository(db);

                // Null when the product does not exist or is not published.
                FTQuiz2_Product product = products.GetPublishedProductById(productId);
                if (product == null) { return ProductNotFound(); }

                model.Product = FTQuiz2_BusinessFormat.ToCard(product);

                foreach (FTQuiz2_Product other in products.GetPublishedProducts())
                {
                    if (other.ProductId == productId) { continue; }
                    if (model.OtherProducts.Count >= FTQuiz2_ProductDetailsViewModel.OtherProductLimit) { break; }
                    model.OtherProducts.Add(FTQuiz2_BusinessFormat.ToCard(other));
                }
            }

            ViewBag.Title = model.Product.Name;
            return View(model);
        }

        private ActionResult ProductNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;     // keep this page; do not let IIS swap in its own 404

            ViewBag.Title = "Product not found";
            return View("NotFound");
        }
    }
}
