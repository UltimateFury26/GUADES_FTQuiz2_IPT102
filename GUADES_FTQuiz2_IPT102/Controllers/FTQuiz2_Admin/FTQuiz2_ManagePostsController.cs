using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Port of Admin/ManagePosts.aspx / .cs: list, create, edit, publish or unpublish, and delete announcements.
    /// FTQuiz2_AdminController is the AdminPage check (and sets the FTQuiz2_admin body class).
    /// The add/edit form opens with ?add=1 or ?edit=ID (the source opened it with a postback).
    /// After every successful change the action redirects to Index?done=code, so a refresh never
    /// repeats it; unknown done codes show nothing (arbitrary text is never displayed).
    /// Repository calls are the original usp_Post_* procedures.
    /// </summary>
    public class FTQuiz2_ManagePostsController : FTQuiz2_AdminController
    {
        private const string MessageKey = "FTQuiz2_ManagePosts_Message";
        private const string MessageKindKey = "FTQuiz2_ManagePosts_MessageKind";

        private const int TitleMaxLength = 150;
        private const int BodyMaxLength = 4000;

        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";
        private const string GoneMessage = "That announcement no longer exists. The list has been refreshed.";
        private const string TitleTooLongMessage = "The title is longer than 150 characters. Shorten it and try again.";
        private const string BodyTooLongMessage = "The message is longer than 4,000 characters. Shorten it and try again.";

        // The only sortable columns (the header buttons of the source Repeater).
        private static readonly string[] SortFields = { "Title", "IsPublished", "CreatedAt", "AuthorName" };

        // =====================================================================================
        // GET: list, or the add/edit form
        // =====================================================================================

        [HttpGet]
        public ActionResult Index(string sort, string dir, string done, string add, int? edit)
        {
            FTQuiz2_ManagePostsViewModel model = NewModel(sort, dir);

            // Outcome of the last redirect: ?done=code (success) or a flashed error.
            string doneText = DoneText(done);
            if (doneText != null)
            {
                model.Message = doneText;
                model.MessageKind = "success";
            }

            string flash = TempData[MessageKey] as string;
            if (flash != null)
            {
                model.Message = flash;
                model.MessageKind = (TempData[MessageKindKey] as string) ?? "error";
            }

            // btnAdd_Click: blank form, list hidden, no message.
            if (add == "1")
            {
                model.Message = null;
                model.ShowForm = true;
                return View(model);
            }

            // LoadForEdit: the post fills the form; "no longer exists" refreshes the list.
            if (edit.HasValue)
            {
                FTQuiz2_PostView post;

                try
                {
                    post = new FTQuiz2_PostRepository().GetById(edit.Value);
                }
                catch (Exception ex) when (IsDatabaseError(ex))
                {
                    model.Message = DatabaseMessage;
                    model.MessageKind = "error";
                    return BindList(model);
                }

                if (post == null)
                {
                    model.Message = GoneMessage;
                    model.MessageKind = "error";
                    return BindList(model);
                }

                model.Message = null;
                model.ShowForm = true;
                model.Form.PostId = post.PostId;
                model.Form.Title = post.Title;
                model.Form.Body = post.Body;
                model.Form.IsPublished = post.IsPublished;
                return View(model);
            }

            return BindList(model);
        }

        // =====================================================================================
        // POST: btnSave_Click
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(FTQuiz2_PostFormViewModel form, string sort, string dir)
        {
            FTQuiz2_ManagePostsViewModel model = NewModel(sort, dir);
            model.ShowForm = true;
            model.Form = form ?? new FTQuiz2_PostFormViewModel();

            // The two RequiredFieldValidators (ErrorMessage text is the source's).
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            string title = (form.Title ?? string.Empty).Trim();
            string body = (form.Body ?? string.Empty).Trim();

            if (title.Length > TitleMaxLength)
            {
                model.Message = TitleTooLongMessage;
                model.MessageKind = "error";
                return View("Index", model);
            }

            if (body.Length > BodyMaxLength)
            {
                model.Message = BodyTooLongMessage;
                model.MessageKind = "error";
                return View("Index", model);
            }

            bool isEdit = form.PostId > 0;
            int affected = 1;

            try
            {
                var repo = new FTQuiz2_PostRepository();

                if (isEdit)
                {
                    affected = repo.Update(form.PostId, title, body, form.IsPublished);
                }
                else
                {
                    repo.Insert(title, body, form.IsPublished, CurrentUserId);
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                model.Message = DatabaseMessage;
                model.MessageKind = "error";
                return View("Index", model);
            }

            if (affected == 0) { return Gone(sort, dir); }

            return Done(isEdit ? FTQuiz2_Constants.PostQuery.Updated : FTQuiz2_Constants.PostQuery.Created, sort, dir);
        }

        // =====================================================================================
        // POST: row buttons (Publish / Unpublish, Delete)
        // =====================================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPublished(int postId, bool publish, string sort, string dir)
        {
            int affected;

            try
            {
                affected = new FTQuiz2_PostRepository().SetPublished(postId, publish);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Flash(DatabaseMessage, sort, dir);
            }

            if (affected == 0) { return Gone(sort, dir); }

            return Done(publish ? FTQuiz2_Constants.PostQuery.Published : FTQuiz2_Constants.PostQuery.Unpublished, sort, dir);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int postId, string sort, string dir)
        {
            int affected;

            try
            {
                affected = new FTQuiz2_PostRepository().Delete(postId);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                return Flash(DatabaseMessage, sort, dir);
            }

            if (affected == 0) { return Gone(sort, dir); }

            return Done(FTQuiz2_Constants.PostQuery.Deleted, sort, dir);
        }

        // =====================================================================================
        // helpers
        // =====================================================================================

        private static FTQuiz2_ManagePostsViewModel NewModel(string sort, string dir)
        {
            return new FTQuiz2_ManagePostsViewModel
            {
                SortField = SortFields.Contains(sort) ? sort : null,
                SortAsc = !string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase)
            };
        }

        // Source BindList: load, sort, count. A database error hides the list, empty state and count.
        private ActionResult BindList(FTQuiz2_ManagePostsViewModel model)
        {
            try
            {
                model.Posts = new FTQuiz2_PostRepository().GetAll();
                model.Loaded = true;
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                model.Message = DatabaseMessage;
                model.MessageKind = "error";
                model.Loaded = false;
                return View(model);
            }

            SortPosts(model.Posts, model.SortField, model.SortAsc);
            return View(model);
        }

        // Source SortPosts: ascending by the chosen column, then reverse for descending.
        private static void SortPosts(List<FTQuiz2_PostView> posts, string field, bool asc)
        {
            if (string.IsNullOrEmpty(field)) { return; }

            IEnumerable<FTQuiz2_PostView> sorted;

            switch (field)
            {
                case "Title":
                    sorted = posts.OrderBy(p => p.Title, StringComparer.OrdinalIgnoreCase); break;
                case "IsPublished":
                    sorted = posts.OrderBy(p => p.IsPublished); break;
                case "AuthorName":
                    sorted = posts.OrderBy(p => p.AuthorName, StringComparer.OrdinalIgnoreCase); break;
                default:
                    sorted = posts.OrderBy(p => p.CreatedAt); break;
            }

            List<FTQuiz2_PostView> result = sorted.ToList();
            if (!asc) { result.Reverse(); }

            posts.Clear();
            posts.AddRange(result);
        }

        // Source RedirectDone: Index?done=code (the current sort is kept, as ViewState kept it).
        private ActionResult Done(string code, string sort, string dir)
        {
            return RedirectToAction("Index", new { done = code, sort = sort, dir = dir });
        }

        private ActionResult Gone(string sort, string dir)
        {
            return Flash(GoneMessage, sort, dir);
        }

        private ActionResult Flash(string text, string sort, string dir)
        {
            TempData[MessageKey] = text;
            TempData[MessageKindKey] = "error";
            return RedirectToAction("Index", new { sort = sort, dir = dir });
        }

        // Source DoneText. Unknown codes give null.
        private static string DoneText(string code)
        {
            switch (code)
            {
                case FTQuiz2_Constants.PostQuery.Created:
                    return "The announcement was created.";
                case FTQuiz2_Constants.PostQuery.Updated:
                    return "The announcement was updated.";
                case FTQuiz2_Constants.PostQuery.Published:
                    return "The announcement is now published. Users can see it on Home.";
                case FTQuiz2_Constants.PostQuery.Unpublished:
                    return "The announcement was unpublished. It is now a draft and hidden from users.";
                case FTQuiz2_Constants.PostQuery.Deleted:
                    return "The announcement was deleted.";
                default:
                    return null;
            }
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
