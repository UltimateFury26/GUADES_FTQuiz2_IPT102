using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Manage Products (Phase 5): list every product (published and unpublished) with its thumbnail, variant,
    /// status and display order; add, edit, replace the image, publish / unpublish, move up / down, delete.
    ///
    /// Admin-only (FTQuiz2_AdminController). The add/edit form opens with ?add=1 or ?edit=ID and hides the list.
    /// Every change is a POST with an anti-forgery token, validated on the server, and ends in a redirect to
    /// Index?done=code so a refresh never repeats it (an unknown code shows nothing). Image rules are the
    /// same as Manage Content: FTQuiz2_FileUploader validates the file and generates the name, the browser
    /// never supplies a path, no new file = the stored image path is passed as null = "keep the current image",
    /// and an old uploaded file is deleted only after the database change succeeded and no other record uses it.
    /// There is no cart, price or checkout: this is a catalogue.
    /// </summary>
    public class FTQuiz2_ManageProductsController : FTQuiz2_AdminController
    {
        private const string MessageKey = "FTQuiz2_ManageProducts_Message";
        private const string MessageKindKey = "FTQuiz2_ManageProducts_MessageKind";
        private const string ErrorKind = "error";
        private const string SuccessKind = "success";

        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";
        private const string GoneMessage = "That product no longer exists. The list has been refreshed.";

        // Display order is renumbered in steps of 10 when a product is moved (10, 20, 30 ...).
        private const int OrderStep = 10;

        // =====================================================================================
        // GET: list, or the add/edit form
        // =====================================================================================

        [HttpGet]
        public ActionResult Index(string done, string add, int? edit)
        {
            var model = new FTQuiz2_ManageProductsViewModel();

            string doneText = DoneText(done);
            if (doneText != null) { Apply(model, doneText, SuccessKind); }

            string flash = TempData[MessageKey] as string;
            if (flash != null) { Apply(model, flash, (TempData[MessageKindKey] as string) ?? ErrorKind); }

            // Blank form for a new product.
            if (add == "1")
            {
                Apply(model, null, null);
                model.ShowForm = true;
                return View(model);
            }

            // The product fills the form; "no longer exists" falls back to the refreshed list.
            if (edit.HasValue)
            {
                FTQuiz2_Product product;

                try
                {
                    product = new FTQuiz2_ProductRepository().GetProductById(edit.Value);
                }
                catch (Exception ex) when (IsDatabaseError(ex))
                {
                    Apply(model, DatabaseMessage, ErrorKind);
                    model.LoadFailed = true;
                    return View(model);
                }

                if (product != null)
                {
                    Apply(model, null, null);
                    model.ShowForm = true;
                    model.IsEdit = true;
                    model.ProductId = product.ProductId;
                    model.ProductName = product.ProductName;
                    model.Flavor = product.Flavor;
                    model.ShortDescription = product.ShortDescription;
                    model.FullDescription = product.FullDescription;
                    model.SortOrder = product.SortOrder.ToString(CultureInfo.InvariantCulture);
                    model.IsPublished = product.IsPublished;
                    model.BackgroundColor = FTQuiz2_BusinessFormat.SafeColor(product.BackgroundColor);
                    model.UseDefaultBackground = model.BackgroundColor.Length == 0;
                    model.CurrentPhotoUrl = FTQuiz2_ImagePath.Resolve(product.ImagePath);
                    return View(model);
                }

                Apply(model, GoneMessage, ErrorKind);
            }

            return BindList(model);
        }

        // =====================================================================================
        // POST: save (create or update)
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(FTQuiz2_ManageProductsViewModel model, HttpPostedFileBase image)
        {
            Reset(model);

            if (!ModelState.IsValid) { return ShowForm(model, null); }

            var repo = new FTQuiz2_ProductRepository();
            bool isEdit = model.ProductId > 0;
            string oldImagePath = null;
            string newImagePath = null;

            if (image != null)
            {
                if (isEdit)
                {
                    try
                    {
                        FTQuiz2_Product existing = repo.GetProductById(model.ProductId);
                        oldImagePath = existing == null ? null : existing.ImagePath;
                    }
                    catch (Exception ex) when (IsDatabaseError(ex)) { return ShowForm(model, DatabaseMessage); }
                }

                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(image);
                if (!upload.Success) { return ShowForm(model, upload.Error); }
                newImagePath = upload.RelativePath;
            }

            var product = new FTQuiz2_Product
            {
                ProductId = model.ProductId,
                ProductName = model.ProductName.Trim(),
                Flavor = NullIfBlank(model.Flavor),
                ShortDescription = NullIfBlank(model.ShortDescription),
                FullDescription = NullIfBlank(model.FullDescription),
                ImagePath = newImagePath,           // null = keep the current image
                IsPublished = model.IsPublished,
                // "" = automatic (clears the colour); "#RRGGBB" = the admin's pick; null = keep the stored colour.
                BackgroundColor = model.UseDefaultBackground
                    ? string.Empty
                    : NullIfBlank(FTQuiz2_BusinessFormat.SafeColor(model.BackgroundColor))
            };

            int? sortOrder = ParseNumber(model.SortOrder);
            int affected = 1;

            try
            {
                if (isEdit) { affected = repo.UpdateProduct(product, sortOrder); }
                else { repo.InsertProduct(product, sortOrder); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newImagePath);
                return ShowForm(model, DatabaseMessage);
            }

            if (affected == 0)
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newImagePath);
                return Fail(GoneMessage);
            }

            if (newImagePath != null) { FTQuiz2_FileUploader.DeleteIfUnreferenced(oldImagePath); }

            return Done(isEdit ? FTQuiz2_Constants.ProductQuery.Updated : FTQuiz2_Constants.ProductQuery.Created);
        }

        // =====================================================================================
        // POST: row buttons
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPublished(int productId, bool publish)
        {
            int affected;

            try
            {
                affected = new FTQuiz2_ProductRepository().SetPublished(productId, publish);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Fail(DatabaseMessage);
            }

            if (affected == 0) { return Fail(GoneMessage); }

            return Done(publish ? FTQuiz2_Constants.ProductQuery.Published : FTQuiz2_Constants.ProductQuery.Unpublished);
        }

        // Moves one product a step up or down in the display order. The whole list is renumbered
        // (10, 20, 30 ...) so products that share the same number can still be moved.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Move(int productId, string direction)
        {
            try
            {
                var repo = new FTQuiz2_ProductRepository();
                List<FTQuiz2_Product> all = repo.GetProducts();      // already in display order

                int index = all.FindIndex(p => p.ProductId == productId);
                if (index < 0) { return Fail(GoneMessage); }

                int target = string.Equals(direction, "up", StringComparison.OrdinalIgnoreCase) ? index - 1 : index + 1;

                // Already first / last (or an unknown direction): nothing to change.
                if (target < 0 || target >= all.Count
                    || (!string.Equals(direction, "up", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(direction, "down", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToAction("Index");
                }

                FTQuiz2_Product moved = all[index];
                all[index] = all[target];
                all[target] = moved;

                for (int i = 0; i < all.Count; i++)
                {
                    int wanted = (i + 1) * OrderStep;
                    if (all[i].SortOrder != wanted) { repo.SetSortOrder(all[i].ProductId, wanted); }
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Fail(DatabaseMessage);
            }

            return Done(FTQuiz2_Constants.ProductQuery.Moved);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int productId)
        {
            try
            {
                var repo = new FTQuiz2_ProductRepository();
                FTQuiz2_Product product = repo.GetProductById(productId);

                if (product == null) { return Fail(GoneMessage); }

                if (repo.DeleteProduct(productId) == 0) { return Fail(GoneMessage); }

                // The row is gone, so its uploaded image can go too (unless another record still uses it).
                FTQuiz2_FileUploader.DeleteIfUnreferenced(product.ImagePath);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Fail(DatabaseMessage);
            }

            return Done(FTQuiz2_Constants.ProductQuery.Deleted);
        }

        // =====================================================================================
        // helpers
        // =====================================================================================

        // Load the admin list (published and unpublished). A database error hides the list and empty state.
        private ActionResult BindList(FTQuiz2_ManageProductsViewModel model)
        {
            try
            {
                model.Products = new FTQuiz2_ProductRepository().GetProducts();
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                Apply(model, DatabaseMessage, ErrorKind);
                model.LoadFailed = true;
            }

            return View(model);
        }

        // Re-display the form after a failed save (what the Admin typed stays; a file input cannot be refilled).
        private ActionResult ShowForm(FTQuiz2_ManageProductsViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }
            model.ShowForm = true;
            model.IsEdit = model.ProductId > 0;

            if (model.IsEdit)
            {
                try
                {
                    FTQuiz2_Product p = new FTQuiz2_ProductRepository().GetProductById(model.ProductId);
                    model.CurrentPhotoUrl = p == null ? string.Empty : FTQuiz2_ImagePath.Resolve(p.ImagePath);
                }
                catch (Exception ex) when (IsDatabaseError(ex)) { }
            }

            return View("Index", model);
        }

        private ActionResult Done(string code)
        {
            return RedirectToAction("Index", new { done = code });
        }

        private ActionResult Fail(string text)
        {
            TempData[MessageKey] = text;
            TempData[MessageKindKey] = ErrorKind;
            return RedirectToAction("Index");
        }

        private static void Apply(FTQuiz2_EditorViewModelBase vm, string message, string kind)
        {
            vm.Message = message;
            vm.MessageKind = message == null ? null : kind;
        }

        // The posted model also carries these display fields; never trust what came back from the browser.
        private static void Reset(FTQuiz2_ManageProductsViewModel model)
        {
            model.Message = null;
            model.MessageKind = null;
            model.LoadFailed = false;
            model.CurrentPhotoUrl = null;
            model.Products = new List<FTQuiz2_Product>();
        }

        // Unknown codes give null (arbitrary text is never displayed).
        private static string DoneText(string code)
        {
            switch (code)
            {
                case FTQuiz2_Constants.ProductQuery.Created:
                    return "The product was added.";
                case FTQuiz2_Constants.ProductQuery.Updated:
                    return "The product was updated.";
                case FTQuiz2_Constants.ProductQuery.Published:
                    return "The product is now published and visible in the catalogue.";
                case FTQuiz2_Constants.ProductQuery.Unpublished:
                    return "The product was unpublished. It is now a draft and hidden from users.";
                case FTQuiz2_Constants.ProductQuery.Moved:
                    return "The display order was updated.";
                case FTQuiz2_Constants.ProductQuery.Deleted:
                    return "The product was deleted.";
                default:
                    return null;
            }
        }

        private static string NullIfBlank(string value)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        private static int? ParseNumber(string text)
        {
            int value;
            return int.TryParse((text ?? string.Empty).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out value)
                ? (int?)value
                : null;
        }

        // EF can wrap the SqlException (e.g. when the connection cannot open).
        private static bool IsDatabaseError(Exception ex)
        {
            for (Exception e = ex; e != null; e = e.InnerException)
            {
                if (e is SqlException) { return true; }
            }
            return false;
        }
    }
}
