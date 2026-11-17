using System;
using System.Collections.Generic;
using System.Globalization;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>A hobby as Home draws it: picture pin when a photo exists on disk, else a coloured text card.</summary>
    public class FTQuiz2_HobbyView
    {
        public string Title { get; set; }
        public string Description { get; set; }   // empty when there is none
        public string ImageUrl { get; set; }      // empty when there is no photo on disk
        public string CardStyle { get; set; }     // background colour for the text card; empty when there is a photo
    }

    /// <summary>A gallery photo that exists on disk.</summary>
    public class FTQuiz2_GalleryPhoto
    {
        public string Url { get; set; }
        public string Alt { get; set; }           // the caption, otherwise "Gallery photo N"
        public string Caption { get; set; }       // empty when there is none
    }

    /// <summary>
    /// Everything Home.aspx / Home.aspx.cs held in fields and helpers. The controller fills it
    /// (same order and rules as the source Page_Load); the view only draws it.
    /// </summary>
    public class FTQuiz2_HomeViewModel
    {
        // Pin cover colours, rotated in order (same palette as the source).
        private static readonly string[] PinColors =
            { "#2f5d62", "#3d4a63", "#5c4a3d", "#47543d", "#614a2e" };

        public string FullName { get; set; }
        public string Tagline { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }

        public List<FTQuiz2_PostView> Posts { get; set; }
        public FTQuiz2_PostView LatestPost { get; set; }
        public string LatestPostDate { get; set; }
        public List<FTQuiz2_Project> Projects { get; set; }
        public List<FTQuiz2_Education> Schools { get; set; }
        public List<FTQuiz2_GalleryPhoto> GalleryPhotos { get; set; }
        public List<FTQuiz2_HobbyView> Hobbies { get; set; }
        public List<string> InterestPreview { get; set; }
        public List<FTQuiz2_SkillBoardViewModel> SkillBoards { get; set; }

        public FTQuiz2_HomeViewModel()
        {
            FullName = string.Empty;
            Tagline = string.Empty;
            Bio = string.Empty;
            AvatarUrl = string.Empty;
            LatestPostDate = string.Empty;
            Posts = new List<FTQuiz2_PostView>();
            Projects = new List<FTQuiz2_Project>();
            Schools = new List<FTQuiz2_Education>();
            GalleryPhotos = new List<FTQuiz2_GalleryPhoto>();
            Hobbies = new List<FTQuiz2_HobbyView>();
            InterestPreview = new List<string>();
            SkillBoards = new List<FTQuiz2_SkillBoardViewModel>();
        }

        /// <summary>Total boards shown on Home, used by the "N boards" label.</summary>
        public int BoardCount
        {
            get
            {
                return (Posts.Count > 0 ? 1 : 0)
                    + (Projects.Count > 0 ? 1 : 0)
                    + SkillBoards.Count
                    + (Schools.Count > 0 ? 1 : 0)
                    + (GalleryPhotos.Count > 0 ? 1 : 0)
                    + (Hobbies.Count > 0 ? 1 : 0);
            }
        }

        /// <summary>Extra class for the Gallery tile cover: one photo fills it, two share it, three or more use the large-plus-stack layout.</summary>
        public string GalleryCoverClass
        {
            get
            {
                if (GalleryPhotos.Count == 1) { return " cover-single"; }
                return GalleryPhotos.Count == 2 ? " cover-duo" : string.Empty;
            }
        }

        /// <summary>Extra class for the Interests tile preview.</summary>
        public string InterestCoverClass
        {
            get { return InterestPreview.Count == 2 ? " cover-duo" : (InterestPreview.Count > 2 ? string.Empty : " cover-single"); }
        }

        // ---- presentation helpers (ports of the source static helpers) ----

        public string PinStyle(int index)
        {
            return "background:" + PinColors[index % PinColors.Length];
        }

        /// <summary>Coloured background when there is no photo, nothing when the photo fills the cover.</summary>
        public string CoverStyle(string photoUrl, int index)
        {
            return photoUrl.Length > 0 ? string.Empty : PinStyle(index);
        }

        public string PhotoUrl(string path) { return FTQuiz2_ImagePath.Resolve(path); }

        public string PinCountText(int count) { return count == 1 ? "1 pin" : count + " pins"; }

        public string FormatDate(DateTime date)
        {
            return date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>"Senior High School · Dr. Carlos S. Lanting College" (level first).</summary>
        public string SchoolLabel(FTQuiz2_Education school)
        {
            return school.Level + " \u00B7 " + school.SchoolName;
        }

        /// <summary>"2022 – 2024", "2024 – Present", "Until 2024", or "" when neither year is known.</summary>
        public string YearsText(FTQuiz2_Education school)
        {
            if (!school.StartYear.HasValue && !school.EndYear.HasValue) { return string.Empty; }

            if (!school.StartYear.HasValue)
            {
                return "Until " + school.EndYear.Value.ToString(CultureInfo.InvariantCulture);
            }

            string end = school.EndYear.HasValue
                ? school.EndYear.Value.ToString(CultureInfo.InvariantCulture)
                : "Present";

            return school.StartYear.Value.ToString(CultureInfo.InvariantCulture) + " \u2013 " + end;
        }

        public string RepoLink(FTQuiz2_Project project) { return FTQuiz2_UrlGuard.SafeHttpUrl(project.RepoUrl); }

        // ---- pin detail modals (same fields, order and hide-when-empty rules as the source) ----

        public FTQuiz2_PinModalViewModel PostModal(FTQuiz2_PostView post)
        {
            var modal = new FTQuiz2_PinModalViewModel { Id = "postModal" + post.PostId, Title = post.Title };
            modal.Lines.Add(new FTQuiz2_PinLine { Text = FormatDate(post.CreatedAt), CssClass = "announcement-date" });
            modal.Lines.Add(new FTQuiz2_PinLine { Text = post.Body, CssClass = "announcement-body" });
            return modal;
        }

        public FTQuiz2_PinModalViewModel ProjectModal(FTQuiz2_Project project)
        {
            var modal = new FTQuiz2_PinModalViewModel { Id = "projectModal" + project.ProjectId, Title = project.Title };
            if (!string.IsNullOrWhiteSpace(project.TechStack)) { modal.Lines.Add(new FTQuiz2_PinLine { Label = "Tech stack:", Text = project.TechStack }); }
            if (!string.IsNullOrWhiteSpace(project.Role)) { modal.Lines.Add(new FTQuiz2_PinLine { Label = "My role:", Text = project.Role }); }
            if (!string.IsNullOrWhiteSpace(project.Details)) { modal.Lines.Add(new FTQuiz2_PinLine { Label = "Details:", Text = project.Details }); }
            modal.LinkUrl = RepoLink(project);
            modal.LinkText = "View Repo";
            return modal;
        }

        public FTQuiz2_PinModalViewModel SchoolModal(FTQuiz2_Education school)
        {
            var modal = new FTQuiz2_PinModalViewModel { Id = "schoolModal" + school.EducationId, Title = school.SchoolName };
            modal.Lines.Add(new FTQuiz2_PinLine { Label = "Level:", Text = school.Level });
            if (!string.IsNullOrWhiteSpace(school.Program)) { modal.Lines.Add(new FTQuiz2_PinLine { Label = "Program:", Text = school.Program }); }
            string years = YearsText(school);
            if (years.Length > 0) { modal.Lines.Add(new FTQuiz2_PinLine { Label = "Years:", Text = years }); }
            if (!string.IsNullOrWhiteSpace(school.Description)) { modal.Lines.Add(new FTQuiz2_PinLine { Text = school.Description }); }
            return modal;
        }

        public FTQuiz2_PinModalViewModel SkillModal(FTQuiz2_Skill skill)
        {
            var modal = new FTQuiz2_PinModalViewModel { Id = "skillModal" + skill.SkillId, Title = skill.Name };
            if (!string.IsNullOrWhiteSpace(skill.Description)) { modal.Lines.Add(new FTQuiz2_PinLine { Text = skill.Description }); }
            return modal;
        }
    }
}
