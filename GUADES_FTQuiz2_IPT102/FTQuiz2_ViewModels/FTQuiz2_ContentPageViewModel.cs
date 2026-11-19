using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>One tab on the Manage Content page.</summary>
    public sealed class FTQuiz2_ContentTab
    {
        public string Key { get; set; }
        public string Title { get; set; }
    }

    /// <summary>Limits, patterns, messages and text helpers shared by the business editors and the product editor.</summary>
    public static class FTQuiz2_ContentFormat
    {
        public const string Dash = "\u2014";

        // Maximum lengths. They match the SQL columns (or are a deliberate, smaller app limit for NVARCHAR(MAX)).
        public const int BusinessNameMax = 100;
        public const int TaglineMax = 200;
        public const int StoryMax = 4000;            // BrandStory is NVARCHAR(MAX); the app keeps it to 4000
        public const int MissionMax = 500;
        public const int VisionMax = 500;
        public const int EmailMax = 100;
        public const int PhoneMax = 20;
        public const int AddressMax = 250;
        public const int UrlMax = 250;
        public const int OwnerNameMax = 100;
        public const int OwnerPositionMax = 100;
        public const int OwnerBioMax = 1000;
        public const int ProductNameMax = 100;
        public const int FlavorMax = 50;
        public const int ShortDescriptionMax = 250;
        public const int FullDescriptionMax = 4000;  // FullDescription is NVARCHAR(MAX); the app keeps it to 4000

        // Patterns (the shape is checked by the pattern, the real rule by FTQuiz2_BusinessFormat / FTQuiz2_UrlGuard).
        public const string SortOrderPattern = @"^[0-9]{1,4}$";                  // 0 - 9999
        public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string PhonePattern = @"^[0-9+()\- ]{7,20}$";
        public const string HttpUrlPattern = @"^https?://\S+$";

        public const string SortOrderMessage = "Enter a whole number from 0 to 9999.";
        public const string EmailMessage = "Enter a valid email, like name@example.com.";
        public const string PhoneMessage = "Use 7 to 20 digits (spaces, +, - and brackets are fine).";
        public const string HttpUrlMessage = "Start the link with http:// or https://";

        /// <summary>The text, or a dash when the optional field is empty.</summary>
        public static string OrDash(string value)
        {
            string text = (value ?? string.Empty).Trim();
            return text.Length == 0 ? Dash : text;
        }

        /// <summary>The start of a longer text for a list, or a dash when there is none.</summary>
        public static string Preview(string value, int length)
        {
            string text = (value ?? string.Empty).Trim();
            if (text.Length == 0) { return Dash; }
            return text.Length <= length ? text : text.Substring(0, length).TrimEnd() + "\u2026";
        }

        /// <summary>URL of an image that really exists on disk, or "" (the page then shows an empty square).</summary>
        public static string ThumbUrl(string path)
        {
            return FTQuiz2_ImagePath.Resolve(path);
        }
    }

    /// <summary>
    /// State every editor shares: the alert above the form, whether the add/edit form is open (list hidden),
    /// and whether the list could be loaded. Each editor's view model also carries that editor's form fields,
    /// so the same class is both the Razor model and the POST model and the field names stay plain
    /// ("FullName", not "Form.FullName").
    /// </summary>
    public abstract class FTQuiz2_EditorViewModelBase
    {
        public string Message { get; set; }
        public string MessageKind { get; set; }      // "success" | "error"

        public bool ShowForm { get; set; }           // true = add/edit form open, list hidden
        public bool IsEdit { get; set; }
        public bool LoadFailed { get; set; }         // database unreachable: list and empty state hidden

        /// <summary>Current photo / image shown above the file input (empty when none is on disk).</summary>
        public string CurrentPhotoUrl { get; set; }
    }

    // ---- Business Profile (singleton row, no list) ----------------------------------------------

    public class FTQuiz2_BusinessProfileEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        [Required(ErrorMessage = "Enter the business name.")]
        [StringLength(FTQuiz2_ContentFormat.BusinessNameMax, ErrorMessage = "The business name can be up to 100 characters.")]
        public string BusinessName { get; set; }

        [StringLength(FTQuiz2_ContentFormat.TaglineMax, ErrorMessage = "The tagline can be up to 200 characters.")]
        public string Tagline { get; set; }

        [StringLength(FTQuiz2_ContentFormat.StoryMax, ErrorMessage = "The brand story can be up to 4000 characters.")]
        public string BrandStory { get; set; }

        [StringLength(FTQuiz2_ContentFormat.MissionMax, ErrorMessage = "The mission can be up to 500 characters.")]
        public string Mission { get; set; }

        [StringLength(FTQuiz2_ContentFormat.VisionMax, ErrorMessage = "The vision can be up to 500 characters.")]
        public string Vision { get; set; }

        /// <summary>True while the brand story is still the seed's "[Placeholder ...]" text.</summary>
        public bool StoryIsPlaceholder
        {
            get { return FTQuiz2_BusinessFormat.IsPlaceholder(BrandStory); }
        }
    }

    // ---- Contact Information (singleton row, no list) -------------------------------------------

    public class FTQuiz2_ContactEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        [StringLength(FTQuiz2_ContentFormat.EmailMax, ErrorMessage = "The email can be up to 100 characters.")]
        [RegularExpression(FTQuiz2_ContentFormat.EmailPattern, ErrorMessage = FTQuiz2_ContentFormat.EmailMessage)]
        public string Email { get; set; }

        [StringLength(FTQuiz2_ContentFormat.PhoneMax, ErrorMessage = "The phone number can be up to 20 characters.")]
        [RegularExpression(FTQuiz2_ContentFormat.PhonePattern, ErrorMessage = FTQuiz2_ContentFormat.PhoneMessage)]
        public string Phone { get; set; }

        [StringLength(FTQuiz2_ContentFormat.AddressMax, ErrorMessage = "The address can be up to 250 characters.")]
        public string Address { get; set; }

        [StringLength(FTQuiz2_ContentFormat.UrlMax, ErrorMessage = "The link can be up to 250 characters.")]
        [RegularExpression(FTQuiz2_ContentFormat.HttpUrlPattern, ErrorMessage = FTQuiz2_ContentFormat.HttpUrlMessage)]
        public string FacebookUrl { get; set; }

        [StringLength(FTQuiz2_ContentFormat.UrlMax, ErrorMessage = "The link can be up to 250 characters.")]
        [RegularExpression(FTQuiz2_ContentFormat.HttpUrlPattern, ErrorMessage = FTQuiz2_ContentFormat.HttpUrlMessage)]
        public string InstagramUrl { get; set; }
    }

    // ---- Owners (list + add/edit form) ----------------------------------------------------------

    public class FTQuiz2_OwnersEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        public List<FTQuiz2_OwnerInfo> Items { get; set; }

        public int OwnerId { get; set; }

        [Required(ErrorMessage = "Enter the owner's full name.")]
        [StringLength(FTQuiz2_ContentFormat.OwnerNameMax, ErrorMessage = "The name can be up to 100 characters.")]
        public string FullName { get; set; }

        [StringLength(FTQuiz2_ContentFormat.OwnerPositionMax, ErrorMessage = "The position can be up to 100 characters.")]
        public string Position { get; set; }

        [StringLength(FTQuiz2_ContentFormat.OwnerBioMax, ErrorMessage = "The bio can be up to 1000 characters.")]
        public string Bio { get; set; }

        [StringLength(FTQuiz2_ContentFormat.EmailMax, ErrorMessage = "The email can be up to 100 characters.")]
        [RegularExpression(FTQuiz2_ContentFormat.EmailPattern, ErrorMessage = FTQuiz2_ContentFormat.EmailMessage)]
        public string Email { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.SortOrderPattern, ErrorMessage = FTQuiz2_ContentFormat.SortOrderMessage)]
        public string SortOrder { get; set; }

        public FTQuiz2_OwnersEditorViewModel() { Items = new List<FTQuiz2_OwnerInfo>(); }

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

    // ---- Brand Media (the two profile images; no list) ------------------------------------------

    public class FTQuiz2_BrandMediaEditorViewModel : FTQuiz2_EditorViewModelBase
    {
        /// <summary>Current homepage hero image (empty when none is set or the file is missing).</summary>
        public string HeroUrl { get; set; }

        /// <summary>Current About page image (empty when none is set or the file is missing).</summary>
        public string AboutUrl { get; set; }

        public FTQuiz2_BrandMediaEditorViewModel()
        {
            HeroUrl = string.Empty;
            AboutUrl = string.Empty;
        }
    }

    // ---- The page -------------------------------------------------------------------------------

    /// <summary>Everything Manage Content needs: the tab strip and the one active editor.</summary>
    public class FTQuiz2_ContentPageViewModel
    {
        public string ActiveTab { get; set; }
        public IList<FTQuiz2_ContentTab> Tabs { get; set; }

        // Only the editor for ActiveTab is filled in.
        public FTQuiz2_BusinessProfileEditorViewModel Business { get; set; }
        public FTQuiz2_ContactEditorViewModel Contact { get; set; }
        public FTQuiz2_OwnersEditorViewModel Owners { get; set; }
        public FTQuiz2_BrandMediaEditorViewModel Media { get; set; }
    }
}
