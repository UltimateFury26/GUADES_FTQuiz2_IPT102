using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>The add/edit form of ManagePosts.aspx (hfPostId, txtTitle, txtBody, chkPublish).</summary>
    public class FTQuiz2_PostFormViewModel
    {
        /// <summary>0 = new announcement (hfPostId.Value = "0").</summary>
        public int PostId { get; set; }

        [Required(ErrorMessage = "Enter a title for the announcement.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Enter the announcement message.")]
        public string Body { get; set; }

        public bool IsPublished { get; set; }
    }

    /// <summary>
    /// Everything ManagePosts.aspx / .cs fed to its markup: the rows, the sort state that lived in
    /// ViewState, the outcome message, the form state (pnlForm) and the list helpers
    /// (Preview, StatusHtml, PostedHtml, SortHeaderText, SortHeaderClass, CountText).
    /// </summary>
    public class FTQuiz2_ManagePostsViewModel
    {
        private const string DateTimeFormat = "MMM d, yyyy h:mm tt";
        private const int ListPreviewLength = 100;

        public List<FTQuiz2_PostView> Posts { get; set; }

        /// <summary>False when the database could not be reached (list, empty state and count are hidden).</summary>
        public bool Loaded { get; set; }

        /// <summary>True while pnlForm is shown; the list and empty state are hidden then.</summary>
        public bool ShowForm { get; set; }
        public FTQuiz2_PostFormViewModel Form { get; set; }

        /// <summary>Active sort column, or null while unsorted (newest-first, the repository's own order).</summary>
        public string SortField { get; set; }
        public bool SortAsc { get; set; }

        public string Message { get; set; }
        public string MessageKind { get; set; }   // "success" | "error"

        public FTQuiz2_ManagePostsViewModel()
        {
            Posts = new List<FTQuiz2_PostView>();
            Form = new FTQuiz2_PostFormViewModel();
            SortAsc = true;
        }

        public bool IsEdit { get { return Form.PostId > 0; } }
        public string FormTitle { get { return IsEdit ? "Edit announcement" : "New announcement"; } }

        // litCount: "1 post, 1 published" / "N posts, M published"; empty when the list could not be loaded.
        public string CountText
        {
            get
            {
                if (!Loaded) { return string.Empty; }

                int published = 0;
                foreach (FTQuiz2_PostView post in Posts)
                {
                    if (post.IsPublished) { published++; }
                }

                string noun = Posts.Count == 1 ? "1 post" : Posts.Count.ToString(CultureInfo.InvariantCulture) + " posts";
                return noun + ", " + published.ToString(CultureInfo.InvariantCulture) + " published";
            }
        }

        /// <summary>Header text plus a ▲/▼ arrow when this column is the active sort.</summary>
        public string SortHeaderText(string label, string field)
        {
            if (SortField != field) { return label; }
            return label + (SortAsc ? " \u25B2" : " \u25BC");
        }

        /// <summary>Highlights whichever header is the active sort.</summary>
        public string SortHeaderClass(string field)
        {
            return SortField == field ? "th-sort active" : "th-sort";
        }

        /// <summary>Clicking the active header reverses the direction; any other header starts ascending.</summary>
        public string NextDir(string field)
        {
            return SortField == field && SortAsc ? "desc" : "asc";
        }

        /// <summary>The start of the message for the list, on one line (source Preview()).</summary>
        public static string Preview(string body)
        {
            string text = Regex.Replace(body ?? string.Empty, @"\s+", " ").Trim();

            return text.Length <= ListPreviewLength
                ? text
                : text.Substring(0, ListPreviewLength).TrimEnd() + "\u2026";
        }

        public string StatusText(bool isPublished) { return isPublished ? "Published" : "Draft"; }
        public string StatusBadgeClass(bool isPublished) { return isPublished ? "badge badge-active" : "badge badge-user"; }

        public string FormatDateTime(DateTime value)
        {
            return value.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}
