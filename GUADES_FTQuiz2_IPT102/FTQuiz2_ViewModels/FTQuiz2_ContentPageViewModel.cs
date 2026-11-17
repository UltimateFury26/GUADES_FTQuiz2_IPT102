using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>One tab on the Manage Content page (source ContentTab: key, label).</summary>
    public sealed class FTQuiz2_ContentTab
    {
        public string Key { get; set; }
        public string Title { get; set; }
    }

    /// <summary>Text helpers the six editors share (the source editors each carried their own copy).</summary>
    public static class FTQuiz2_ContentFormat
    {
        public const string Dash = "\u2014";

        // Validation patterns copied from the source RangeValidators / RegularExpressionValidators.
        public const string YearPattern = @"^(19[5-9][0-9]|20[0-9]{2}|2100)$";   // 1950 - 2100
        public const string SortOrderPattern = @"^[0-9]{1,4}$";                  // 0 - 9999
        public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string PhonePattern = @"^[0-9+()\- ]{7,20}$";
        public const string HttpUrlPattern = @"^https?://\S+$";

        public const string YearMessage = "Enter a year between 1950 and 2100.";
        public const string SortOrderMessage = "Enter a whole number from 0 to 9999.";
        public const string HttpUrlMessage = "Start the link with http:// or https://";

        public static readonly string[] SkillCategories =
        {
            FTQuiz2_Constants.SkillCategories.Frontend,
            FTQuiz2_Constants.SkillCategories.Backend,
            FTQuiz2_Constants.SkillCategories.Tools
        };

        /// <summary>The text, or a dash when the optional field is empty.</summary>
        public static string OrDash(string value)
        {
            string text = (value ?? string.Empty).Trim();
            return text.Length == 0 ? Dash : text;
        }

        /// <summary>The start of a description for the list, or a dash when there is none.</summary>
        public static string Preview(string value, int length)
        {
            string text = (value ?? string.Empty).Trim();
            if (text.Length == 0) { return Dash; }
            return text.Length <= length ? text : text.Substring(0, length).TrimEnd() + "\u2026";
        }

        /// <summary>The board a category feeds: "Tools" is shown as "Tools &amp; Platforms".</summary>
        public static string CategoryLabel(string category)
        {
            return category == FTQuiz2_Constants.SkillCategories.Tools ? "Tools & Platforms" : category;
        }

        /// <summary>"2022 – 2024", "2024 – Present", or a dash when the years are not known.</summary>
        public static string FormatYears(short? startYear, short? endYear)
        {
            if (!startYear.HasValue) { return Dash; }
            return startYear.Value + " \u2013 " + (endYear.HasValue ? endYear.Value.ToString() : "Present");
        }

        /// <summary>URL of an image that really exists on disk, or "" (the page then shows an empty square).</summary>
        public static string ThumbUrl(string path)
        {
            return FTQuiz2_ImagePath.Resolve(path);
        }
    }

    /// <summary>
    /// State every editor shares: the alert above the form (source pnlMessage), whether the add/edit
    /// form is open (pnlForm), and whether the list could be loaded. Each editor's view model also
    /// carries that editor's form fields, so the same class is both the Razor model and the POST
    /// model (the MyAccount pattern), and the field names stay plain ("SchoolName", not "Form.SchoolName").
    /// </summary>
    public abstract class FTQuiz2_EditorViewModelBase
    {
        public string Message { get; set; }
        public string MessageKind { get; set; }      // "success" | "error"

        public bool ShowForm { get; set; }           // true = add/edit form open, list hidden
        public bool IsEdit { get; set; }
        public bool LoadFailed { get; set; }         // database unreachable: list and empty state hidden

        /// <summary>Current avatar / photo shown above the file input (empty when none is on disk).</summary>
        public string CurrentPhotoUrl { get; set; }
    }

    // ---- Personal Info (singleton row, no list) -------------------------------------------------

    public class FTQuiz2_PersonalInfoEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        [Required(ErrorMessage = "Enter the full name.")]
        public string FullName { get; set; }

        public string Tagline { get; set; }
        public string Bio { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.EmailPattern, ErrorMessage = "Enter a valid email, like name@example.com.")]
        public string Email { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.PhonePattern, ErrorMessage = "Use 7 to 20 digits (spaces, +, - and brackets are fine).")]
        public string Phone { get; set; }

        public string Address { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.HttpUrlPattern, ErrorMessage = FTQuiz2_ContentFormat.HttpUrlMessage)]
        public string GitHubUrl { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.HttpUrlPattern, ErrorMessage = FTQuiz2_ContentFormat.HttpUrlMessage)]
        public string LinkedInUrl { get; set; }
    }

    // ---- Education ------------------------------------------------------------------------------

    public class FTQuiz2_EducationEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        public List<FTQuiz2_Education> Items { get; set; }

        public int EducationId { get; set; }

        [Required(ErrorMessage = "Enter the school name.")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "Enter the level.")]
        public string Level { get; set; }

        public string Program { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.YearPattern, ErrorMessage = FTQuiz2_ContentFormat.YearMessage)]
        public string StartYear { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.YearPattern, ErrorMessage = FTQuiz2_ContentFormat.YearMessage)]
        public string EndYear { get; set; }

        public string Description { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.SortOrderPattern, ErrorMessage = FTQuiz2_ContentFormat.SortOrderMessage)]
        public string SortOrder { get; set; }

        public FTQuiz2_EducationEditorViewModel() { Items = new List<FTQuiz2_Education>(); }
    }

    // ---- Skills ---------------------------------------------------------------------------------

    public class FTQuiz2_SkillsEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        public List<FTQuiz2_Skill> Items { get; set; }

        public int SkillId { get; set; }

        [Required(ErrorMessage = "Enter the skill name.")]
        public string Name { get; set; }

        public string Category { get; set; }
        public string Description { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.SortOrderPattern, ErrorMessage = FTQuiz2_ContentFormat.SortOrderMessage)]
        public string SortOrder { get; set; }

        public FTQuiz2_SkillsEditorViewModel() { Items = new List<FTQuiz2_Skill>(); }

        /// <summary>The three category choices, in source order (Frontend, Backend, Tools &amp; Platforms).</summary>
        public IEnumerable<SelectListItem> CategoryOptions
        {
            get
            {
                foreach (string category in FTQuiz2_ContentFormat.SkillCategories)
                {
                    yield return new SelectListItem
                    {
                        Value = category,
                        Text = FTQuiz2_ContentFormat.CategoryLabel(category),
                        Selected = category == Category
                    };
                }
            }
        }
    }

    // ---- Hobbies --------------------------------------------------------------------------------

    public class FTQuiz2_HobbiesEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        public List<FTQuiz2_Hobby> Items { get; set; }

        public int HobbyId { get; set; }

        [Required(ErrorMessage = "Enter a title for this hobby.")]
        public string Title { get; set; }

        public string Description { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.SortOrderPattern, ErrorMessage = FTQuiz2_ContentFormat.SortOrderMessage)]
        public string SortOrder { get; set; }

        public FTQuiz2_HobbiesEditorViewModel() { Items = new List<FTQuiz2_Hobby>(); }
    }

    // ---- Projects (no photo, so no upload) ------------------------------------------------------

    public class FTQuiz2_ProjectsEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        public List<FTQuiz2_Project> Items { get; set; }

        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Enter the project title.")]
        public string Title { get; set; }

        public string TechStack { get; set; }
        public string Role { get; set; }
        public string Details { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.HttpUrlPattern, ErrorMessage = FTQuiz2_ContentFormat.HttpUrlMessage)]
        public string RepoUrl { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.SortOrderPattern, ErrorMessage = FTQuiz2_ContentFormat.SortOrderMessage)]
        public string SortOrder { get; set; }

        public FTQuiz2_ProjectsEditorViewModel() { Items = new List<FTQuiz2_Project>(); }
    }

    // ---- Gallery --------------------------------------------------------------------------------

    public class FTQuiz2_GalleryEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        public List<FTQuiz2_GalleryItem> Items { get; set; }

        public int GalleryId { get; set; }
        public string Caption { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.SortOrderPattern, ErrorMessage = FTQuiz2_ContentFormat.SortOrderMessage)]
        public string SortOrder { get; set; }

        public FTQuiz2_GalleryEditorViewModel() { Items = new List<FTQuiz2_GalleryItem>(); }

        public string PhotoHint
        {
            get
            {
                return IsEdit
                    ? "JPG, PNG or GIF, up to 5 MB. Leave empty to keep the current photo."
                    : "JPG, PNG or GIF, up to 5 MB.";
            }
        }
    }

    // ---- The page -------------------------------------------------------------------------------

    /// <summary>Everything ManageContent.aspx fed to its markup: the tab strip and the one active editor.</summary>
    public class FTQuiz2_ContentPageViewModel
    {
        public string ActiveTab { get; set; }
        public IList<FTQuiz2_ContentTab> Tabs { get; set; }

        // Only the editor for ActiveTab is filled in (the source loaded one user control per request).
        public FTQuiz2_PersonalInfoEditorViewModel Personal { get; set; }
        public FTQuiz2_EducationEditorViewModel Education { get; set; }
        public FTQuiz2_SkillsEditorViewModel Skills { get; set; }
        public FTQuiz2_HobbiesEditorViewModel Hobbies { get; set; }
        public FTQuiz2_ProjectsEditorViewModel Projects { get; set; }
        public FTQuiz2_GalleryEditorViewModel Gallery { get; set; }
    }
}
