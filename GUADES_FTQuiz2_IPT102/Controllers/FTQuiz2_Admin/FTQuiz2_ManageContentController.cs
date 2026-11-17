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
    /// Port of Admin/ManageContent.aspx + the six Admin/Controls/*Editor.ascx(.cs) pairs.
    /// ?tab= picks the editor (unknown or missing = first tab). The add/edit form is opened with
    /// ?add=1 or ?edit=ID (the source opened it with a postback) and the list is hidden while it is open.
    /// A successful save or delete redirects to ?tab=X&amp;saved=1 (post/redirect/get), exactly like
    /// ContentEditorBase.RedirectAfterSave. FTQuiz2_AdminController is the AdminPage check.
    /// Repository calls are the original stored procedures (usp_*_Insert/Update/Delete): NULL sort order =
    /// "last" on insert / "keep" on update; a null image path = "keep the current image".
    /// </summary>
    public class FTQuiz2_ManageContentController : FTQuiz2_AdminController
    {
        private const string MessageKey = "FTQuiz2_ManageContent_Message";
        private const string MessageKindKey = "FTQuiz2_ManageContent_MessageKind";
        private const string ErrorKind = "error";
        private const string SuccessKind = "success";

        private const string SavedMessage = "Your changes were saved.";
        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";

        private const string BioTooLongMessage = "The bio is longer than 2000 characters. Shorten it and try again.";
        private const string EducationGoneMessage = "That school no longer exists. The list has been refreshed.";
        private const string EducationDescriptionTooLongMessage = "The description is longer than 1000 characters. Shorten it and try again.";
        private const string SkillGoneMessage = "That skill no longer exists. The list has been refreshed.";
        private const string SkillDescriptionTooLongMessage = "The description is longer than 1500 characters. Shorten it and try again.";
        private const string HobbyGoneMessage = "That hobby no longer exists. The list has been refreshed.";
        private const string HobbyDescriptionTooLongMessage = "The description is longer than 500 characters. Shorten it and try again.";
        private const string ProjectGoneMessage = "That project no longer exists. The list has been refreshed.";
        private const string ProjectDetailsTooLongMessage = "The details are longer than 2000 characters. Shorten them and try again.";
        private const string ProjectBadLinkMessage = "The repository link must start with http:// or https://";
        private const string GalleryGoneMessage = "That photo no longer exists. The list has been refreshed.";
        private const string GalleryPhotoRequiredMessage = "Choose a photo to add.";

        private static readonly FTQuiz2_ContentTab[] Tabs =
        {
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Personal,  Title = "Personal Info" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Education, Title = "Education" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Skills,    Title = "Skills" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Hobbies,   Title = "Hobbies" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Projects,  Title = "Projects" },
            new FTQuiz2_ContentTab { Key = FTQuiz2_Constants.ContentTabs.Gallery,   Title = "Gallery" }
        };

        // =====================================================================================
        // GET: the page (tab strip + the one active editor)
        // =====================================================================================

        [HttpGet]
        public ActionResult Index(string tab, string saved, string add, int? edit)
        {
            string active = FindTab(tab);
            FTQuiz2_ContentPageViewModel page = NewPage(active);

            // Source WasJustSaved, plus the outcome of a delete / "no longer exists" redirect.
            string message = null;
            string kind = null;

            if (saved == "1") { message = SavedMessage; kind = SuccessKind; }

            string flash = TempData[MessageKey] as string;
            if (flash != null)
            {
                message = flash;
                kind = (TempData[MessageKindKey] as string) ?? ErrorKind;
            }

            bool openAdd = add == "1";

            switch (active)
            {
                case FTQuiz2_Constants.ContentTabs.Education: page.Education = LoadEducation(message, kind, openAdd, edit); break;
                case FTQuiz2_Constants.ContentTabs.Skills:    page.Skills = LoadSkills(message, kind, openAdd, edit); break;
                case FTQuiz2_Constants.ContentTabs.Hobbies:   page.Hobbies = LoadHobbies(message, kind, openAdd, edit); break;
                case FTQuiz2_Constants.ContentTabs.Projects:  page.Projects = LoadProjects(message, kind, openAdd, edit); break;
                case FTQuiz2_Constants.ContentTabs.Gallery:   page.Gallery = LoadGallery(message, kind, openAdd, edit); break;
                default:                                      page.Personal = LoadPersonal(message, kind); break;
            }

            return View(page);
        }

        // =====================================================================================
        // Personal Info (PersonalInfoEditor.ascx.cs)
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavePersonalInfo(FTQuiz2_PersonalInfoEditorViewModel model, HttpPostedFileBase photo)
        {
            ClearMessage(model);

            string gitHub = NullIfBlank(model.GitHubUrl);
            string linkedIn = NullIfBlank(model.LinkedInUrl);

            // The pattern checks the shape; this makes sure a link really is a usable http(s) address.
            if (gitHub != null && FTQuiz2_UrlGuard.SafeHttpUrl(gitHub).Length == 0)
            {
                ModelState.AddModelError("GitHubUrl", FTQuiz2_ContentFormat.HttpUrlMessage);
            }

            if (linkedIn != null && FTQuiz2_UrlGuard.SafeHttpUrl(linkedIn).Length == 0)
            {
                ModelState.AddModelError("LinkedInUrl", FTQuiz2_ContentFormat.HttpUrlMessage);
            }

            if (!ModelState.IsValid) { return ShowPersonal(model, null); }

            if ((model.Bio ?? string.Empty).Length > 2000) { return ShowPersonal(model, BioTooLongMessage); }

            var repo = new FTQuiz2_ContentRepository();
            string oldAvatarPath;

            try
            {
                FTQuiz2_PersonalInfo current = repo.GetPersonalInfo();
                oldAvatarPath = current == null ? null : current.AvatarPath;
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return ShowPersonal(model, DatabaseMessage);
            }

            string newAvatarPath = null;

            if (photo != null)
            {
                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(photo);
                if (!upload.Success) { return ShowPersonal(model, upload.Error); }
                newAvatarPath = upload.RelativePath;
            }

            try
            {
                repo.UpdatePersonalInfo(new FTQuiz2_PersonalInfo
                {
                    FullName = model.FullName.Trim(),
                    Tagline = NullIfBlank(model.Tagline),
                    Bio = NullIfBlank(model.Bio),
                    Email = NullIfBlank(model.Email),
                    Phone = NullIfBlank(model.Phone),
                    Address = NullIfBlank(model.Address),
                    GitHubUrl = gitHub,
                    LinkedInUrl = linkedIn,
                    AvatarPath = newAvatarPath          // null = keep the current photo
                });
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newAvatarPath);
                return ShowPersonal(model, DatabaseMessage);
            }

            // The old upload goes only after the save succeeded.
            if (newAvatarPath != null) { FTQuiz2_FileUploader.DeleteIfUploaded(oldAvatarPath); }

            return Saved(FTQuiz2_Constants.ContentTabs.Personal);
        }

        // =====================================================================================
        // Education (EducationEditor.ascx.cs)
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveEducation(FTQuiz2_EducationEditorViewModel model, HttpPostedFileBase photo)
        {
            ClearMessage(model);

            // Source cvYears: an end year needs a start year and cannot be earlier than it.
            int? start = ParseNumber(model.StartYear);
            int? end = ParseNumber(model.EndYear);

            if (end.HasValue && !start.HasValue)
            {
                ModelState.AddModelError("Years", "Enter a start year too, or clear the end year.");
            }
            else if (start.HasValue && end.HasValue && end.Value < start.Value)
            {
                ModelState.AddModelError("Years", "The end year can't be before the start year.");
            }

            if (!ModelState.IsValid) { return ShowEducation(model, null); }

            if ((model.Description ?? string.Empty).Length > 1000) { return ShowEducation(model, EducationDescriptionTooLongMessage); }

            var repo = new FTQuiz2_ContentRepository();
            bool isEdit = model.EducationId > 0;
            string oldPhotoPath = null;
            string newPhotoPath = null;

            if (photo != null)
            {
                if (isEdit)
                {
                    try
                    {
                        FTQuiz2_Education existing = repo.GetEducationById(model.EducationId);
                        oldPhotoPath = existing == null ? null : existing.PhotoPath;
                    }
                    catch (Exception ex) when (IsDatabaseError(ex)) { return ShowEducation(model, DatabaseMessage); }
                }

                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(photo);
                if (!upload.Success) { return ShowEducation(model, upload.Error); }
                newPhotoPath = upload.RelativePath;
            }

            var entry = new FTQuiz2_Education
            {
                EducationId = model.EducationId,
                SchoolName = model.SchoolName.Trim(),
                Level = model.Level.Trim(),
                Program = NullIfBlank(model.Program),
                StartYear = (short?)start,
                EndYear = (short?)end,
                Description = NullIfBlank(model.Description),
                PhotoPath = newPhotoPath
            };

            int? sortOrder = ParseNumber(model.SortOrder);
            int affected = 1;

            try
            {
                if (isEdit) { affected = repo.UpdateEducation(entry, sortOrder); }
                else { repo.InsertEducation(entry, sortOrder); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newPhotoPath);
                return ShowEducation(model, DatabaseMessage);
            }

            if (affected == 0)
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newPhotoPath);
                return Back(FTQuiz2_Constants.ContentTabs.Education, EducationGoneMessage);
            }

            if (newPhotoPath != null) { FTQuiz2_FileUploader.DeleteIfUploaded(oldPhotoPath); }

            return Saved(FTQuiz2_Constants.ContentTabs.Education);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEducation(int id)
        {
            try
            {
                var repo = new FTQuiz2_ContentRepository();
                FTQuiz2_Education entry = repo.GetEducationById(id);

                if (entry == null) { return Back(FTQuiz2_Constants.ContentTabs.Education, EducationGoneMessage); }

                if (repo.DeleteEducation(id) > 0) { FTQuiz2_FileUploader.DeleteIfUploaded(entry.PhotoPath); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Back(FTQuiz2_Constants.ContentTabs.Education, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Education);
        }

        // =====================================================================================
        // Skills (SkillsEditor.ascx.cs)
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSkill(FTQuiz2_SkillsEditorViewModel model)
        {
            ClearMessage(model);

            if (Array.IndexOf(FTQuiz2_ContentFormat.SkillCategories, model.Category) < 0)
            {
                ModelState.AddModelError("Category", "Choose a category.");
            }

            if (!ModelState.IsValid) { return ShowSkills(model, null); }

            if ((model.Description ?? string.Empty).Length > 1500) { return ShowSkills(model, SkillDescriptionTooLongMessage); }

            var skill = new FTQuiz2_Skill
            {
                SkillId = model.SkillId,
                Name = model.Name.Trim(),
                Category = model.Category,
                Description = NullIfBlank(model.Description)
            };

            bool isEdit = model.SkillId > 0;
            int? sortOrder = ParseNumber(model.SortOrder);
            int affected = 1;

            try
            {
                var repo = new FTQuiz2_ContentRepository();
                if (isEdit) { affected = repo.UpdateSkill(skill, sortOrder); }
                else { repo.InsertSkill(skill, sortOrder); }
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { return ShowSkills(model, DatabaseMessage); }

            if (affected == 0) { return Back(FTQuiz2_Constants.ContentTabs.Skills, SkillGoneMessage); }

            return Saved(FTQuiz2_Constants.ContentTabs.Skills);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSkill(int id)
        {
            try
            {
                if (new FTQuiz2_ContentRepository().DeleteSkill(id) == 0)
                {
                    return Back(FTQuiz2_Constants.ContentTabs.Skills, SkillGoneMessage);
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Back(FTQuiz2_Constants.ContentTabs.Skills, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Skills);
        }

        // =====================================================================================
        // Hobbies (HobbiesEditor.ascx.cs)
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveHobby(FTQuiz2_HobbiesEditorViewModel model, HttpPostedFileBase photo)
        {
            ClearMessage(model);

            if (!ModelState.IsValid) { return ShowHobbies(model, null); }

            if ((model.Description ?? string.Empty).Length > 500) { return ShowHobbies(model, HobbyDescriptionTooLongMessage); }

            var repo = new FTQuiz2_ContentRepository();
            bool isEdit = model.HobbyId > 0;
            string oldImagePath = null;
            string newImagePath = null;

            if (photo != null)
            {
                if (isEdit)
                {
                    try
                    {
                        FTQuiz2_Hobby existing = repo.GetHobbyById(model.HobbyId);
                        oldImagePath = existing == null ? null : existing.ImagePath;
                    }
                    catch (Exception ex) when (IsDatabaseError(ex)) { return ShowHobbies(model, DatabaseMessage); }
                }

                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(photo);
                if (!upload.Success) { return ShowHobbies(model, upload.Error); }
                newImagePath = upload.RelativePath;
            }

            var entry = new FTQuiz2_Hobby
            {
                HobbyId = model.HobbyId,
                Title = model.Title.Trim(),
                Description = NullIfBlank(model.Description),
                ImagePath = newImagePath
            };

            int? sortOrder = ParseNumber(model.SortOrder);
            int affected = 1;

            try
            {
                if (isEdit) { affected = repo.UpdateHobby(entry, sortOrder); }
                else { repo.InsertHobby(entry, sortOrder); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newImagePath);
                return ShowHobbies(model, DatabaseMessage);
            }

            if (affected == 0)
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newImagePath);
                return Back(FTQuiz2_Constants.ContentTabs.Hobbies, HobbyGoneMessage);
            }

            if (newImagePath != null) { FTQuiz2_FileUploader.DeleteIfUploaded(oldImagePath); }

            return Saved(FTQuiz2_Constants.ContentTabs.Hobbies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteHobby(int id)
        {
            try
            {
                var repo = new FTQuiz2_ContentRepository();
                FTQuiz2_Hobby entry = repo.GetHobbyById(id);

                if (entry == null) { return Back(FTQuiz2_Constants.ContentTabs.Hobbies, HobbyGoneMessage); }

                if (repo.DeleteHobby(id) > 0) { FTQuiz2_FileUploader.DeleteIfUploaded(entry.ImagePath); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Back(FTQuiz2_Constants.ContentTabs.Hobbies, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Hobbies);
        }

        // =====================================================================================
        // Projects (ProjectsEditor.ascx.cs) - no photo
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveProject(FTQuiz2_ProjectsEditorViewModel model)
        {
            ClearMessage(model);

            if (!ModelState.IsValid) { return ShowProjects(model, null); }

            if ((model.Details ?? string.Empty).Length > 2000) { return ShowProjects(model, ProjectDetailsTooLongMessage); }

            // The pattern checks the shape; this makes sure the link really is a usable http(s) address.
            string repoUrl = NullIfBlank(model.RepoUrl);
            if (repoUrl != null && FTQuiz2_UrlGuard.SafeHttpUrl(repoUrl).Length == 0)
            {
                return ShowProjects(model, ProjectBadLinkMessage);
            }

            var project = new FTQuiz2_Project
            {
                ProjectId = model.ProjectId,
                Title = model.Title.Trim(),
                TechStack = NullIfBlank(model.TechStack),
                Role = NullIfBlank(model.Role),
                Details = NullIfBlank(model.Details),
                RepoUrl = repoUrl
            };

            bool isEdit = model.ProjectId > 0;
            int? sortOrder = ParseNumber(model.SortOrder);
            int affected = 1;

            try
            {
                var repo = new FTQuiz2_ContentRepository();
                if (isEdit) { affected = repo.UpdateProject(project, sortOrder); }
                else { repo.InsertProject(project, sortOrder); }
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { return ShowProjects(model, DatabaseMessage); }

            if (affected == 0) { return Back(FTQuiz2_Constants.ContentTabs.Projects, ProjectGoneMessage); }

            return Saved(FTQuiz2_Constants.ContentTabs.Projects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProject(int id)
        {
            try
            {
                if (new FTQuiz2_ContentRepository().DeleteProject(id) == 0)
                {
                    return Back(FTQuiz2_Constants.ContentTabs.Projects, ProjectGoneMessage);
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Back(FTQuiz2_Constants.ContentTabs.Projects, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Projects);
        }

        // =====================================================================================
        // Gallery (GalleryEditor.ascx.cs) - a new photo is required; editing may keep the old one
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveGalleryItem(FTQuiz2_GalleryEditorViewModel model, HttpPostedFileBase photo)
        {
            ClearMessage(model);

            if (!ModelState.IsValid) { return ShowGallery(model, null); }

            var repo = new FTQuiz2_ContentRepository();
            bool isEdit = model.GalleryId > 0;

            if (!isEdit && photo == null) { return ShowGallery(model, GalleryPhotoRequiredMessage); }

            string oldImagePath = null;
            string newImagePath = null;

            if (photo != null)
            {
                if (isEdit)
                {
                    try
                    {
                        FTQuiz2_GalleryItem existing = repo.GetGalleryById(model.GalleryId);
                        oldImagePath = existing == null ? null : existing.ImagePath;
                    }
                    catch (Exception ex) when (IsDatabaseError(ex)) { return ShowGallery(model, DatabaseMessage); }
                }

                FTQuiz2_UploadResult upload = FTQuiz2_FileUploader.SaveImage(photo);
                if (!upload.Success) { return ShowGallery(model, upload.Error); }
                newImagePath = upload.RelativePath;
            }

            var entry = new FTQuiz2_GalleryItem
            {
                GalleryId = model.GalleryId,
                ImagePath = newImagePath,
                Caption = NullIfBlank(model.Caption)
            };

            int? sortOrder = ParseNumber(model.SortOrder);
            int affected = 1;

            try
            {
                if (isEdit) { affected = repo.UpdateGalleryItem(entry, sortOrder); }
                else { repo.InsertGalleryItem(entry, sortOrder); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newImagePath);
                return ShowGallery(model, DatabaseMessage);
            }

            if (affected == 0)
            {
                FTQuiz2_FileUploader.DeleteIfUploaded(newImagePath);
                return Back(FTQuiz2_Constants.ContentTabs.Gallery, GalleryGoneMessage);
            }

            if (newImagePath != null) { FTQuiz2_FileUploader.DeleteIfUploaded(oldImagePath); }

            return Saved(FTQuiz2_Constants.ContentTabs.Gallery);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGalleryItem(int id)
        {
            try
            {
                var repo = new FTQuiz2_ContentRepository();
                FTQuiz2_GalleryItem entry = repo.GetGalleryById(id);

                if (entry == null) { return Back(FTQuiz2_Constants.ContentTabs.Gallery, GalleryGoneMessage); }

                if (repo.DeleteGalleryItem(id) > 0) { FTQuiz2_FileUploader.DeleteIfUploaded(entry.ImagePath); }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Back(FTQuiz2_Constants.ContentTabs.Gallery, DatabaseMessage);
            }

            return Saved(FTQuiz2_Constants.ContentTabs.Gallery);
        }

        // =====================================================================================
        // GET loaders (one per editor; the source's Page_Load / LoadForm / LoadForEdit / BindList)
        // =====================================================================================

        private FTQuiz2_PersonalInfoEditorViewModel LoadPersonal(string message, string kind)
        {
            var vm = new FTQuiz2_PersonalInfoEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                FTQuiz2_PersonalInfo info = new FTQuiz2_ContentRepository().GetPersonalInfo();

                if (info != null)
                {
                    vm.FullName = info.FullName;
                    vm.Tagline = info.Tagline;
                    vm.Bio = info.Bio;
                    vm.Email = info.Email;
                    vm.Phone = info.Phone;
                    vm.Address = info.Address;
                    vm.GitHubUrl = info.GitHubUrl;
                    vm.LinkedInUrl = info.LinkedInUrl;
                    vm.CurrentPhotoUrl = FTQuiz2_ImagePath.Resolve(info.AvatarPath);
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); }

            return vm;
        }

        private FTQuiz2_EducationEditorViewModel LoadEducation(string message, string kind, bool add, int? edit)
        {
            var vm = new FTQuiz2_EducationEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                var repo = new FTQuiz2_ContentRepository();

                if (add) { vm.ShowForm = true; return vm; }

                if (edit.HasValue)
                {
                    FTQuiz2_Education e = repo.GetEducationById(edit.Value);

                    if (e != null)
                    {
                        vm.ShowForm = true;
                        vm.IsEdit = true;
                        vm.EducationId = e.EducationId;
                        vm.SchoolName = e.SchoolName;
                        vm.Level = e.Level;
                        vm.Program = e.Program;
                        vm.StartYear = e.StartYear.HasValue ? e.StartYear.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                        vm.EndYear = e.EndYear.HasValue ? e.EndYear.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                        vm.Description = e.Description;
                        vm.SortOrder = e.SortOrder.ToString(CultureInfo.InvariantCulture);
                        vm.CurrentPhotoUrl = FTQuiz2_ImagePath.Resolve(e.PhotoPath);
                        return vm;
                    }

                    Apply(vm, EducationGoneMessage, ErrorKind);
                }

                vm.Items = repo.GetEducation();
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); vm.LoadFailed = true; }

            return vm;
        }

        private FTQuiz2_SkillsEditorViewModel LoadSkills(string message, string kind, bool add, int? edit)
        {
            var vm = new FTQuiz2_SkillsEditorViewModel { Category = FTQuiz2_ContentFormat.SkillCategories[0] };
            Apply(vm, message, kind);

            try
            {
                var repo = new FTQuiz2_ContentRepository();

                if (add) { vm.ShowForm = true; return vm; }

                if (edit.HasValue)
                {
                    FTQuiz2_Skill s = repo.GetSkillById(edit.Value);

                    if (s != null)
                    {
                        vm.ShowForm = true;
                        vm.IsEdit = true;
                        vm.SkillId = s.SkillId;
                        vm.Name = s.Name;
                        vm.Category = s.Category;
                        vm.Description = s.Description;
                        vm.SortOrder = s.SortOrder.ToString(CultureInfo.InvariantCulture);
                        return vm;
                    }

                    Apply(vm, SkillGoneMessage, ErrorKind);
                }

                vm.Items = repo.GetSkills();
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); vm.LoadFailed = true; }

            return vm;
        }

        private FTQuiz2_HobbiesEditorViewModel LoadHobbies(string message, string kind, bool add, int? edit)
        {
            var vm = new FTQuiz2_HobbiesEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                var repo = new FTQuiz2_ContentRepository();

                if (add) { vm.ShowForm = true; return vm; }

                if (edit.HasValue)
                {
                    FTQuiz2_Hobby h = repo.GetHobbyById(edit.Value);

                    if (h != null)
                    {
                        vm.ShowForm = true;
                        vm.IsEdit = true;
                        vm.HobbyId = h.HobbyId;
                        vm.Title = h.Title;
                        vm.Description = h.Description;
                        vm.SortOrder = h.SortOrder.ToString(CultureInfo.InvariantCulture);
                        vm.CurrentPhotoUrl = FTQuiz2_ImagePath.Resolve(h.ImagePath);
                        return vm;
                    }

                    Apply(vm, HobbyGoneMessage, ErrorKind);
                }

                vm.Items = repo.GetHobbies();
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); vm.LoadFailed = true; }

            return vm;
        }

        private FTQuiz2_ProjectsEditorViewModel LoadProjects(string message, string kind, bool add, int? edit)
        {
            var vm = new FTQuiz2_ProjectsEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                var repo = new FTQuiz2_ContentRepository();

                if (add) { vm.ShowForm = true; return vm; }

                if (edit.HasValue)
                {
                    FTQuiz2_Project p = repo.GetProjectById(edit.Value);

                    if (p != null)
                    {
                        vm.ShowForm = true;
                        vm.IsEdit = true;
                        vm.ProjectId = p.ProjectId;
                        vm.Title = p.Title;
                        vm.TechStack = p.TechStack;
                        vm.Role = p.Role;
                        vm.Details = p.Details;
                        vm.RepoUrl = p.RepoUrl;
                        vm.SortOrder = p.SortOrder.ToString(CultureInfo.InvariantCulture);
                        return vm;
                    }

                    Apply(vm, ProjectGoneMessage, ErrorKind);
                }

                vm.Items = repo.GetProjects();
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); vm.LoadFailed = true; }

            return vm;
        }

        private FTQuiz2_GalleryEditorViewModel LoadGallery(string message, string kind, bool add, int? edit)
        {
            var vm = new FTQuiz2_GalleryEditorViewModel();
            Apply(vm, message, kind);

            try
            {
                var repo = new FTQuiz2_ContentRepository();

                if (add) { vm.ShowForm = true; return vm; }

                if (edit.HasValue)
                {
                    FTQuiz2_GalleryItem g = repo.GetGalleryById(edit.Value);

                    if (g != null)
                    {
                        vm.ShowForm = true;
                        vm.IsEdit = true;
                        vm.GalleryId = g.GalleryId;
                        vm.Caption = g.Caption;
                        vm.SortOrder = g.SortOrder.ToString(CultureInfo.InvariantCulture);
                        vm.CurrentPhotoUrl = FTQuiz2_ImagePath.Resolve(g.ImagePath);
                        return vm;
                    }

                    Apply(vm, GalleryGoneMessage, ErrorKind);
                }

                vm.Items = repo.GetGallery();
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { Apply(vm, DatabaseMessage, ErrorKind); vm.LoadFailed = true; }

            return vm;
        }

        // =====================================================================================
        // Re-display a form after a failed save (the form stays open with what the Admin typed;
        // a file input can never be refilled, same as the source FileUpload).
        // =====================================================================================

        private ActionResult ShowPersonal(FTQuiz2_PersonalInfoEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }

            try
            {
                FTQuiz2_PersonalInfo info = new FTQuiz2_ContentRepository().GetPersonalInfo();
                model.CurrentPhotoUrl = info == null ? string.Empty : FTQuiz2_ImagePath.Resolve(info.AvatarPath);
            }
            catch (Exception ex) when (IsDatabaseError(ex)) { }

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Personal);
            page.Personal = model;
            return View("Index", page);
        }

        private ActionResult ShowEducation(FTQuiz2_EducationEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }
            model.ShowForm = true;
            model.IsEdit = model.EducationId > 0;

            if (model.IsEdit)
            {
                try
                {
                    FTQuiz2_Education e = new FTQuiz2_ContentRepository().GetEducationById(model.EducationId);
                    model.CurrentPhotoUrl = e == null ? string.Empty : FTQuiz2_ImagePath.Resolve(e.PhotoPath);
                }
                catch (Exception ex) when (IsDatabaseError(ex)) { }
            }

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Education);
            page.Education = model;
            return View("Index", page);
        }

        private ActionResult ShowSkills(FTQuiz2_SkillsEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }
            model.ShowForm = true;
            model.IsEdit = model.SkillId > 0;

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Skills);
            page.Skills = model;
            return View("Index", page);
        }

        private ActionResult ShowHobbies(FTQuiz2_HobbiesEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }
            model.ShowForm = true;
            model.IsEdit = model.HobbyId > 0;

            if (model.IsEdit)
            {
                try
                {
                    FTQuiz2_Hobby h = new FTQuiz2_ContentRepository().GetHobbyById(model.HobbyId);
                    model.CurrentPhotoUrl = h == null ? string.Empty : FTQuiz2_ImagePath.Resolve(h.ImagePath);
                }
                catch (Exception ex) when (IsDatabaseError(ex)) { }
            }

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Hobbies);
            page.Hobbies = model;
            return View("Index", page);
        }

        private ActionResult ShowProjects(FTQuiz2_ProjectsEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }
            model.ShowForm = true;
            model.IsEdit = model.ProjectId > 0;

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Projects);
            page.Projects = model;
            return View("Index", page);
        }

        private ActionResult ShowGallery(FTQuiz2_GalleryEditorViewModel model, string message)
        {
            if (message != null) { Apply(model, message, ErrorKind); }
            model.ShowForm = true;
            model.IsEdit = model.GalleryId > 0;

            if (model.IsEdit)
            {
                try
                {
                    FTQuiz2_GalleryItem g = new FTQuiz2_ContentRepository().GetGalleryById(model.GalleryId);
                    model.CurrentPhotoUrl = g == null ? string.Empty : FTQuiz2_ImagePath.Resolve(g.ImagePath);
                }
                catch (Exception ex) when (IsDatabaseError(ex)) { }
            }

            FTQuiz2_ContentPageViewModel page = NewPage(FTQuiz2_Constants.ContentTabs.Gallery);
            page.Gallery = model;
            return View("Index", page);
        }

        // =====================================================================================
        // Small helpers
        // =====================================================================================

        private static FTQuiz2_ContentPageViewModel NewPage(string activeTab)
        {
            return new FTQuiz2_ContentPageViewModel { ActiveTab = activeTab, Tabs = Tabs };
        }

        /// <summary>Finds the tab for a key; an unknown or missing key shows the first tab (source FindTab).</summary>
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

        // Source RedirectAfterSave: back to the same tab with the saved flag.
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

        /// <summary>Reads a whole number from text; blank or non-numeric text gives null (the patterns report the latter).</summary>
        private static int? ParseNumber(string text)
        {
            int value;
            return int.TryParse((text ?? string.Empty).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out value)
                ? (int?)value
                : null;
        }

        // Source caught SqlException. EF can wrap it (e.g. when the connection cannot open).
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
