using System;
using System.Collections.Generic;
using System.Globalization;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// Everything ManageUsers.aspx / .cs fed to its markup: the rows, the sort state that lived in
    /// ViewState, the outcome message, and the header / badge / date helpers.
    /// </summary>
    public class FTQuiz2_ManageUsersViewModel
    {
        private const string DateFormat = "MMM d, yyyy";
        private const string DateTimeFormat = "MMM d, yyyy h:mm tt";

        public List<FTQuiz2_User> Users { get; set; }

        /// <summary>False when the database could not be reached (the table and empty state are both hidden).</summary>
        public bool Loaded { get; set; }

        public int CurrentUserId { get; set; }

        /// <summary>Active sort column, or null while unsorted (source: unsorted until a header is clicked).</summary>
        public string SortField { get; set; }
        public bool SortAsc { get; set; }

        /// <summary>One-line outcome of the last action, shown under the page heading.</summary>
        public string Message { get; set; }
        public string MessageKind { get; set; }   // "success" | "error"

        public FTQuiz2_ManageUsersViewModel()
        {
            Users = new List<FTQuiz2_User>();
            SortAsc = true;
        }

        // litCount: "1 user" / "N users", empty when the list could not be loaded.
        public string CountText
        {
            get
            {
                if (!Loaded) { return string.Empty; }
                return Users.Count == 1 ? "1 user" : Users.Count.ToString(CultureInfo.InvariantCulture) + " users";
            }
        }

        /// <summary>Header text plus a ▲/▼ arrow when this column is the active sort.</summary>
        public string SortHeaderText(string label, string field)
        {
            if (SortField != field) { return label; }
            return label + (SortAsc ? " ▲" : " ▼");
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

        public string RoleBadgeClass(string role)
        {
            return role == FTQuiz2_Constants.Roles.Admin ? "badge badge-admin" : "badge badge-user";
        }

        public string StatusBadgeClass(string status)
        {
            return status == FTQuiz2_Constants.Status.Active ? "badge badge-active" : "badge badge-inactive";
        }

        public string FormatDate(DateTime value)
        {
            return value.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        public string FormatLastLogin(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString(DateTimeFormat, CultureInfo.InvariantCulture) : "Never";
        }
    }
}
