using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Manage Business Content (Phase 5). Four tabs: Business Profile, Contact Information, Owners
    /// Information and Brand Media. ?tab= picks the editor (unknown or missing = first tab). The owner
    /// add/edit form opens with ?add=1 or ?edit=ID and hides the list. A successful save or delete redirects
    /// to ?tab=X&amp;saved=1 (post/redirect/get). Products have their own page (FTQuiz2_ManageProductsController).
    ///
    /// Every action is admin-only (FTQuiz2_AdminController). Every state change is a POST with an
    /// anti-forgery token and is validated on the server. Uploads go through FTQuiz2_FileUploader
    /// (extension + content type + file signature + 5 MB, generated file name, fixed folder); a path typed
    /// or posted by the browser is never stored. When no new file is uploaded the stored image path is
    /// passed as null, which the stored procedure treats as "keep the current image". An old uploaded file is
    /// deleted only after the database change succeeded AND no other business record still uses it.
    /// </summary>
    public class FTQuiz2_ManageContentController : FTQuiz2_AdminController
    {
        private const string MessageKey = "FTQuiz2_ManageContent_Message";
        private const string MessageKindKey = "FTQuiz2_ManageContent_MessageKind";
        private const string ErrorKind = "error";
        private const string SuccessKind = "success";

        private const string SavedMessage = "Your changes were saved.";
        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";

        private const string BadEmailMessage = FTQuiz2_ContentFormat.EmailMessage;
        private const string OwnerGoneMessage = "That owner no longer exists. The list has been refreshed.";
        private const string LastOwnerMessage = "Keep at least one owner record. Edit this one instead of deleting it.";
        private const string NoMediaChosenMessage = "Choose a hero image or an About image to upload.";
        private const string ProfileMissingMessage = "The business profile record is missing. Save the Business Profile tab first.";

        private static readonly FTQuiz2_ContentTab[] Tabs =
        {
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Business, Title = "Business Profile" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Contact,  Title = "Contact Information" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Owners,   Title = "Owners Information" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Media,    Title = "Brand Media" }
        };

        // =====================================================================================
        // GET: the page (tab strip + the one active editor)
        // =====================================================================================

        [HttpGet]
        public ActionResult Index(string tab, string saved, string add, int? edit)
        {
            string active = FindTab(tab);
            FTQuiz2_ContentPageViewModel page = NewPage(active);

            string message = null;
            string kind = null;

            if (saved == "1") { message = SavedMessage; kind = SuccessKind; }

            string flash = TempData[MessageKey] as string;
            if (flash != null)
            {
                message = flash;
                kind = (TempData[MessageKindKey] as string) ?? ErrorKind;
            }

            switch (active)
            {
                case FTQuiz2_Constants.ContentTabs.Contact: page.Contact = LoadContact(message, kind); break;
                case FTQuiz2_Constants.ContentTabs.Owners:  page.Owners = LoadOwners(message, kind, add == "1", edit); break;
                case FTQuiz2_Constants.ContentTabs.Media:   page.Media = LoadMedia(message, kind); break;
                default:                                    page.Business = LoadBusiness(message, kind); break;
            }

            return View(page);
        }

        // =====================================================================================
        // Business Profile
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveBusinessProfile(FTQuiz2_BusinessProfileEditorViewModel model)
        {
            ClearMessage(model);

            if (!ModelState.IsValid) { return ShowBusiness(model, null); }

            try
            {
                // Image paths stay null here: the Brand Media tab owns the images and null = keep them.
                new FTQuiz2_BusinessRepository().UpdateBusinessProfile(new FTQuiz2_BusinessProfile
                {
                    BusinessName = model.BusinessName.Trim(),
                    Tagline = NullIfBlank(model.Tagline),
                    BrandStory = NullIfBlank(model.BrandStory),
                    Mission = NullIfBlank(model.Mission),
                    Vision = NullIfBlank(model.Vision),
                    AboutImagePath = null,
                    HeroImagePath = null
                });
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return ShowBusiness(model, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Business);
        }

        // =====================================================================================
        // Contact Information
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveContact(FTQuiz2_ContactEditorViewModel model)
        {
            ClearMessage(model);

            string email = NullIfBlank(model.Email);
            string facebook = NullIfBlank(model.FacebookUrl);
            string instagram = NullIfBlank(model.InstagramUrl);

            // The patterns check the shape; these make sure the value is really safe to show as a link.
            if (email != null && FTQuiz2_BusinessFormat.SafeEmail(email).Length == 0)
            {
                ModelState.AddModelError("Email", BadEmailMessage);
            }

            if (facebook != null && FTQuiz2_UrlGuard.SafeHttpUrl(facebook).Length == 0)
            {
                ModelState.AddModelError("FacebookUrl", FTQuiz2_ContentFormat.HttpUrlMessage);
            }

            if (instagram != null && FTQuiz2_UrlGuard.SafeHttpUrl(instagram).Length == 0)
            {
                ModelState.AddModelError("InstagramUrl", FTQuiz2_ContentFormat.HttpUrlMessage);
            }

            if (!ModelState.IsValid) { return ShowContact(model, null); }

            try
            {
                new FTQuiz2_BusinessRepository().UpdateContactInfo(new FTQuiz2_ContactInfo
                {
                    Email = email,
                    Phone = NullIfBlank(model.Phone),
                    Address = NullIfBlank(model.Address),
                    FacebookUrl = facebook,
                    InstagramUrl = instagram
                });
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return ShowContact(model, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Contact);
        }

        // =====================================================================================
        // Owners Information
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOwner(FTQuiz2_OwnersEditorViewModel model, HttpPostedFileBase photo)
        {
            ClearMessage(model);

            string email = NullIfBlank(model.Email);

            if (email != null && FTQuiz2_BusinessFormat.SafeEmail(email).Length == 0)
            {
                ModelState.AddModelError("Email", BadEmailMessage);
            }

            if (!ModelState.IsValid) { return ShowOwners(model, null); }

            var repo = new FTQuiz2_BusinessRepository();
            bool isEdit = model.OwnerId > 0;
            string oldPhotoPath = null;
            string newPhotoPath = null;

            if (photo != null)
            {
                if (isEdit)
                {
                    try
                    {
                        FTQuiz2_OwnerInfo existing = repo.GetOwnerById(model.OwnerId);
                        oldPhotoPath = existing == null ? null : existing.PhotoPath;
                    }
                    catch (Exception ex) when (IsDatabaseError(ex)) { return ShowOwners(model, DatabaseMessage); }
                }

                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(photo);
                if (!upload.Success) { return ShowOwners(model, upload.Error); }
                newPhotoPath = upload.RelativePath;
            }

            var owner = new FTQuiz2_OwnerInfo
            {
                OwnerId = model.OwnerId,
                FullName = model.FullName.Trim(),
                Position = NullIfBlank(model.Position),
                Bio = NullIfBlank(model.Bio),
                Email = email,
                PhotoPath = newPhotoPath            // null = keep the current photo
            };

            int? sortOrder = ParseNumber(model.SortOrder);
            int affected = 1;

            try
            {
                if (isEdit) { affected = repo.UpdateOwner(owner, sortOrder); }
                else { repo.InsertOwner(owner, sortOrder); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newPhotoPath);
                return ShowOwners(model, DatabaseMessage);
            }

            if (affected == 0)
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newPhotoPath);
                return Back(FTQuiz2_Constants.ContentTabs.Owners, OwnerGoneMessage);
            }

            // The old upload goes only after the save succeeded and nothing else uses it.
            if (newPhotoPath != null) { FTQuiz2_FileUploader.DeleteIfUnreferenced(oldPhotoPath); }

            return Saved(FTQuiz2_Constants.ContentTabs.Owners);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteOwner(int id)
        {
            try
            {
                var repo = new FTQuiz2_BusinessRepository();
                FTQuiz2_OwnerInfo owner = repo.GetOwnerById(id);

                if (owner == null) { return Back(FTQuiz2_Constants.ContentTabs.Owners, OwnerGoneMessage); }

                // The Owners page is a required part of the site: never leave it with no record at all.
                if (repo.GetOwners().Count <= 1) { return Back(FTQuiz2_Constants.ContentTabs.Owners, LastOwnerMessage); }

                if (repo.DeleteOwner(id) > 0) { FTQuiz2_FileUploader.DeleteIfUnreferenced(owner.PhotoPath); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Back(FTQuiz2_Constants.ContentTabs.Owners, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Owners);
        }

        // =====================================================================================
        // Brand Media: the homepage hero image and the About page image
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveBrandMedia(HttpPostedFileBase heroImage, HttpPostedFileBase aboutImage)
        {
            bool hasHero = heroImage != null && heroImage.ContentLength > 0;
            bool hasAbout = aboutImage != null && aboutImage.ContentLength > 0;

            if (!hasHero && !hasAbout) { return ShowMedia(NoMediaChosenMessage); }

            var repo = new FTQuiz2_BusinessRepository();
            FTQuiz2_BusinessProfile profile;

            try
            {
                profile = repo.GetBusinessProfile();
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return ShowMedia(DatabaseMessage);
            }

            if (profile == null) { return ShowMedia(ProfileMissingMessage); }

            string newHero = null;
            string newAbout = null;

            if (hasHero)
            {
                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(heroImage);
                if (!upload.Success) { return ShowMedia("Hero image: " + upload.Error); }
                newHero = upload.RelativePath;
            }

            if (hasAbout)
            {
                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(aboutImage);
                if (!upload.Success)
                {
                    FTQuiz2_FileUploader.DeleteIfUploaded(newHero);
                    return ShowMedia("About image: " + upload.Error);
                }
                newAbout = upload.RelativePath;
            }

            try
            {
                // The procedure rewrites the text columns too, so the current text is sent back unchanged;
                // only the image paths that were just uploaded are set (null = keep the current image).
                repo.UpdateBusinessProfile(new FTQuiz2_BusinessProfile
                {
                    BusinessName = profile.BusinessName,
                    Tagline = profile.Tagline,
                    BrandStory = profile.BrandStory,
                    Mission = profile.Mission,
                    Vision = profile.Vision,
                    HeroImagePath = newHero,
                    AboutImagePath = newAbout
                });
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newHero);
                FTQuiz2_FileUploader.DeleteIfUploaded(newAbout);
                return ShowMedia(DatabaseMessage);
            }

            if (newHero != null) { FTQuiz2_FileUploader.DeleteIfUnreferenced(profile.HeroImagePath); }
            if (newAbout != null) { FTQuiz2_FileUploader.DeleteIfUnreferenced(profile.AboutImagePath); }

            return Saved(FTQuiz2_Constants.ContentTabs.Media);
        }

        // =====================================================================================
        // GET loaders
        // =====================================================================================

        private FTQuiz2_BusinessProfileEditorViewModel LoadBusiness(string message, string kind)
        {
            var vm = new FTQuiz2_BusinessProfileEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                FTQuiz2_BusinessProfile profile = new FTQuiz2_BusinessRepository().GetBusinessProfile();

                if (profile != null)
                {
                    vm.BusinessName = profile.BusinessName;
                    vm.Tagline = profile.Tagline;
                    vm.BrandStory = profile.BrandStory;
                    vm.Mission = profile.Mission;
                    vm.Vision = profile.Vision;
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); }

            return vm;
        }

        private FTQuiz2_ContactEditorViewModel LoadContact(string message, string kind)
        {
            var vm = new FTQuiz2_ContactEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                FTQuiz2_ContactInfo contact = new FTQuiz2_BusinessRepository().GetContactInfo();

                if (contact != null)
                {
                    vm.Email = contact.Email;
                    vm.Phone = contact.Phone;
                    vm.Address = contact.Address;
                    vm.FacebookUrl = contact.FacebookUrl;
                    vm.InstagramUrl = contact.InstagramUrl;
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); }

            return vm;
        }

        private FTQuiz2_OwnersEditorViewModel LoadOwners(string message, string kind, bool add, int? edit)
        {
            var vm = new FTQuiz2_OwnersEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                var repo = new FTQuiz2_BusinessRepository();

                if (add) { vm.ShowForm = true; return vm; }

                if (edit.HasValue)
                {
                    FTQuiz2_OwnerInfo o = repo.GetOwnerById(edit.Value);

                    if (o != null)
                    {
                        vm.ShowForm = true;
                        vm.IsEdit = true;
                        vm.OwnerId = o.OwnerId;
                        vm.FullName = o.FullName;
                        vm.Position = o.Position;
                        vm.Bio = o.Bio;
                        vm.Email = o.Email;
                        vm.SortOrder = o.SortOrder.ToString(CultureInfo.InvariantCulture);
                        vm.CurrentPhotoUrl = FTQuiz2_ImagePath.Resolve(o.PhotoPath);
                        return vm;
                    }

                    Apply(vm, OwnerGoneMessage, ErrorKind);
                }

                vm.Items = repo.GetOwners();
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); vm.LoadFailed = true; }

            return vm;
        }

        private FTQuiz2_BrandMediaEditorViewModel LoadMedia(string message, string kind)
        {
            var vm = new FTQuiz2_BrandMediaEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                FTQuiz2_BusinessProfile profile = new FTQuiz2_BusinessRepository().GetBusinessProfile();

                if (profile != null)
                {
                    vm.HeroUrl = FTQuiz2_ImagePath.Resolve(profile.HeroImagePath);
                    vm.AboutUrl = FTQuiz2_ImagePath.Resolve(profile.AboutImagePath);
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); }

            return vm;
        }

        // =====================================================================================
        // Re-display a form after a failed save (the form stays open with what the Admin typed;
        // a file input can never be refilled).
        // =====================================================================================

        private ActionResult ShowBusiness(FTQuiz2_BusinessProfileEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Business);
            page.Business = model;
            return View("Index", page);
        }

        private ActionResult ShowContact(FTQuiz2_ContactEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Contact);
            page.Contact = model;
            return View("Index", page);
        }

        private ActionResult ShowOwners(FTQuiz2_OwnersEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }
            model.ShowForm = true;
            model.IsEdit = model.OwnerId > 0;

            if (model.IsEdit)
            {
                try
                {
                    FTQuiz2_OwnerInfo o = new FTQuiz2_BusinessRepository().GetOwnerById(model.OwnerId);
                    model.CurrentPhotoUrl = o == null ? string.Empty : FTQuiz2_ImagePath.Resolve(o.PhotoPath);
                }
                catch (Exception ex) when (IsDatabaseError(ex)) { }
            }

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Owners);
            page.Owners = model;
            return View("Index", page);
        }

        private ActionResult ShowMedia(string message)
        {
            FTQuiz2_BrandMediaEditorViewModel vm = LoadMedia(null, null);
            Apply(vm, message, ErrorKind);

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Media);
            page.Media = vm;
            return View("Index", page);
        }

        // =====================================================================================
        // Small helpers
        // =====================================================================================

        private static FTQuiz2_ContentPageViewModel NewPage(string activeTab)
        {
            return new FTQuiz2_ContentPageViewModel { ActiveTab = activeTab, Tabs = Tabs };
        }

        /// <summary>Finds the tab for a key; an unknown or missing key shows the first tab.</summary>
        private static string FindTab(string key)
        {
            foreach (FTQuiz2_ContentTab tab in Tabs)
            {
                if (string.Equals(tab.Key, key, StringComparison.OrdinalIgnoreCase)) { return tab.Key; }
            }

            return Tabs[0].Key;
        }

        private static void Apply(FTQuiz2_EditorViewModelBase vm, string message, string kind)
        {
            vm.Message = message;
            vm.MessageKind = message == null ? null : (kind ?? ErrorKind);
        }

        // The posted model also carries these display fields; never trust what came back from the browser.
        private static void ClearMessage(FTQuiz2_EditorViewModelBase vm)
        {
            vm.Message = null;
            vm.MessageKind = null;
            vm.LoadFailed = false;
            vm.CurrentPhotoUrl = null;
        }

        // Back to the same tab with the saved flag (post/redirect/get).
        private ActionResult Saved(string tab)
        {
            return RedirectToAction("Index", new RouteValueDictionary
            {
                { FTQuiz2_Constants.ContentQuery.Tab, tab },
                { FTQuiz2_Constants.ContentQuery.Saved, "1" }
            });
        }

        // "That row no longer exists" / database trouble on a delete: back to the refreshed list with the message.
        private ActionResult Back(string tab, string text)
        {
            TempData[MessageKey] = text;
            TempData[MessageKindKey] = ErrorKind;
            return RedirectToAction("Index", new RouteValueDictionary { { FTQuiz2_Constants.ContentQuery.Tab, tab } });
        }

        /// <summary>Trims the text and returns null when nothing is left, so empty optional fields are stored as NULL.</summary>
        private static string NullIfBlank(string value)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        /// <summary>Reads a whole number from text; blank or non-numeric text gives null (the pattern reports the latter).</summary>
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
