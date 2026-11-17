using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Port of Home.aspx.cs (a BasePage: login required and the account re-checked as Active on
    /// every request). Loading order and rules follow the source Page_Load.
    /// </summary>
    [FTQuiz2_RequireLogin]
    public class HomeController : FTQuiz2_BaseController
    {
        // Pin cover colours for hobby text cards (same palette as the view model).
        private static readonly string[] PinColors =
            { "#2f5d62", "#3d4a63", "#5c4a3d", "#47543d", "#614a2e" };

        public ActionResult Index()
        {
            var content = new FTQuiz2_ContentRepository();
            var model = new FTQuiz2_HomeViewModel();

            LoadProfile(model, content);
            LoadAnnouncements(model);
            model.Projects = content.GetProjects();
            LoadSkillBoards(model, content);
            model.Schools = content.GetEducation();
            LoadGallery(model, content);
            LoadHobbies(model, content);
            RecordVisit();

            ViewBag.Title = "Home";
            return View(model);
        }

        // Source RecordVisit: only regular Users are counted, and a failure must never break the page
        // (FTQuiz2_VisitRepository already swallows SqlException).
        private void RecordVisit()
        {
            if (IsAdmin) { return; }
            new FTQuiz2_VisitRepository().TryRecordVisit(CurrentUserId);
        }

        // usp_PersonalInfo_Get. Empty values stay empty; the view hides any header part with nothing to show.
        private static void LoadProfile(FTQuiz2_HomeViewModel model, FTQuiz2_ContentRepository content)
        {
            FTQuiz2_PersonalInfo info = content.GetPersonalInfo();
            if (info == null) { return; }

            model.FullName = info.FullName;
            model.Tagline = info.Tagline ?? string.Empty;
            model.Bio = info.Bio ?? string.Empty;
            model.AvatarUrl = FTQuiz2_ImagePath.Resolve(info.AvatarPath);
        }

        // usp_Post_GetPublished: newest first, so the first one is the latest.
        private void LoadAnnouncements(FTQuiz2_HomeViewModel model)
        {
            model.Posts = new FTQuiz2_PostRepository().GetPublished();
            if (model.Posts.Count == 0) { return; }

            model.LatestPost = model.Posts[0];
            model.LatestPostDate = model.FormatDate(model.LatestPost.CreatedAt);
        }

        // Skills sorted onto the Frontend, Backend and Tools boards by Category; empty boards are left out.
        private static void LoadSkillBoards(FTQuiz2_HomeViewModel model, FTQuiz2_ContentRepository content)
        {
            List<FTQuiz2_Skill> skills = content.GetSkills();

            foreach (FTQuiz2_SkillBoardViewModel board in CreateSkillBoards())
            {
                board.Skills = skills.Where(skill => skill.Category == board.Category).ToList();

                if (board.Skills.Count > 0) { model.SkillBoards.Add(board); }
            }
        }

        // usp_Gallery_GetAll, keeping only photos whose file exists on disk.
        private static void LoadGallery(FTQuiz2_HomeViewModel model, FTQuiz2_ContentRepository content)
        {
            List<FTQuiz2_GalleryItem> entries = content.GetGallery();

            for (int i = 0; i < entries.Count; i++)
            {
                string url = FTQuiz2_ImagePath.Resolve(entries[i].ImagePath);
                if (url.Length == 0) { continue; }

                string caption = entries[i].Caption;
                model.GalleryPhotos.Add(new FTQuiz2_GalleryPhoto
                {
                    Url = url,
                    Alt = string.IsNullOrWhiteSpace(caption) ? "Gallery photo " + (i + 1) : caption,
                    Caption = string.IsNullOrWhiteSpace(caption) ? string.Empty : caption.Trim()
                });
            }
        }

        // usp_Hobby_GetAll. A hobby whose photo is missing keeps its place as a text card;
        // only hobbies with a real photo feed the tile preview (up to four).
        private static void LoadHobbies(FTQuiz2_HomeViewModel model, FTQuiz2_ContentRepository content)
        {
            List<FTQuiz2_Hobby> entries = content.GetHobbies();

            for (int i = 0; i < entries.Count; i++)
            {
                string imageUrl = FTQuiz2_ImagePath.Resolve(entries[i].ImagePath);

                model.Hobbies.Add(new FTQuiz2_HobbyView
                {
                    Title = entries[i].Title,
                    Description = entries[i].Description ?? string.Empty,
                    ImageUrl = imageUrl,
                    CardStyle = imageUrl.Length > 0 ? string.Empty : "background:" + PinColors[i % PinColors.Length]
                });

                if (imageUrl.Length > 0 && model.InterestPreview.Count < 4)
                {
                    model.InterestPreview.Add(imageUrl);
                }
            }
        }

        // The fixed look of the three skill boards, one per category (texts and icons from the source).
        private static List<FTQuiz2_SkillBoardViewModel> CreateSkillBoards()
        {
            return new List<FTQuiz2_SkillBoardViewModel>
            {
                new FTQuiz2_SkillBoardViewModel
                {
                    Category = FTQuiz2_Constants.SkillCategories.Frontend,
                    Key = "frontend",
                    Title = "Frontend",
                    Description = "Client-side languages and tools I use to build interfaces.",
                    Color = "#2f5d62",
                    IconMarkup = "<rect x=\"3\" y=\"4\" width=\"18\" height=\"16\" rx=\"2\" /><path d=\"M3 9h18\" />"
                },
                new FTQuiz2_SkillBoardViewModel
                {
                    Category = FTQuiz2_Constants.SkillCategories.Backend,
                    Key = "backend",
                    Title = "Backend",
                    Description = "Server-side languages and data tools I work with.",
                    Color = "#3d4a63",
                    IconMarkup = "<rect x=\"3\" y=\"4\" width=\"18\" height=\"6\" rx=\"1.5\" /><rect x=\"3\" y=\"14\" width=\"18\" height=\"6\" rx=\"1.5\" /><path d=\"M7 7h.01\" /><path d=\"M7 17h.01\" />"
                },
                new FTQuiz2_SkillBoardViewModel
                {
                    Category = FTQuiz2_Constants.SkillCategories.Tools,
                    Key = "tools",
                    Title = "Tools & Platforms",
                    Description = "Software and platforms I use day-to-day for development.",
                    Color = "#5c4a3d",
                    IconMarkup = "<path d=\"M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-2.8 2.8-2-2z\" />"
                }
            };
        }
    }
}
