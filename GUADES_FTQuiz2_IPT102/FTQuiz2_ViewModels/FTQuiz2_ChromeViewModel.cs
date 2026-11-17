using System;
using System.Text.RegularExpressions;
using System.Web;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// Everything Site.Master / Site.Master.cs supplied to the shell: owner details from the
    /// database, the logged-in account, and which rail link is active. Loaded at most once per
    /// request (ForRequest) and only by the signed-in layout, so Login/Signup/Error never touch
    /// the database for chrome (the source Auth.Master did not either).
    /// </summary>
    public class FTQuiz2_ChromeViewModel
    {
        private const string ItemsKey = "FTQuiz2_ChromeViewModel";

        // Defaults mirror Site.Master.cs, so none of these is ever null.
        public string OwnerName { get; private set; }
        public string Initials { get; private set; }
        public string Email { get; private set; }
        public string PhoneDisplay { get; private set; }
        public string Address { get; private set; }
        public string GitHubUrl { get; private set; }
        public string LinkedInUrl { get; private set; }

        public string AccountName { get { return FTQuiz2_CurrentUser.Username; } }
        public string AccountRole { get { return FTQuiz2_CurrentUser.Role; } }
        public bool IsAdmin { get { return FTQuiz2_CurrentUser.IsAdmin; } }

        private FTQuiz2_ChromeViewModel()
        {
            OwnerName = "Portfolio";
            Initials = "P";
            Email = string.Empty;
            PhoneDisplay = string.Empty;
            Address = string.Empty;
            GitHubUrl = string.Empty;
            LinkedInUrl = string.Empty;
        }

        /// <summary>The shared model for this request (loaded on first use).</summary>
        public static FTQuiz2_ChromeViewModel ForRequest()
        {
            HttpContext ctx = HttpContext.Current;
            FTQuiz2_ChromeViewModel model = ctx != null ? ctx.Items[ItemsKey] as FTQuiz2_ChromeViewModel : null;
            if (model != null) { return model; }

            model = From(new FTQuiz2_ContentRepository().GetPersonalInfo());
            if (ctx != null) { ctx.Items[ItemsKey] = model; }
            return model;
        }

        /// <summary>Port of SiteMaster.LoadOwner. A missing PersonalInfo row keeps the defaults.</summary>
        public static FTQuiz2_ChromeViewModel From(FTQuiz2_PersonalInfo info)
        {
            FTQuiz2_ChromeViewModel model = new FTQuiz2_ChromeViewModel();
            if (info == null) { return model; }

            model.OwnerName = info.FullName;
            model.Initials = GetInitials(info.FullName);
            model.Email = info.Email ?? string.Empty;
            model.PhoneDisplay = FormatPhone(info.Phone);
            model.Address = info.Address ?? string.Empty;
            model.GitHubUrl = FTQuiz2_UrlGuard.SafeHttpUrl(info.GitHubUrl);
            model.LinkedInUrl = FTQuiz2_UrlGuard.SafeHttpUrl(info.LinkedInUrl);
            return model;
        }

        /// <summary>First letter of the logged-in username, for the avatar circle ("?" when none).</summary>
        public string AccountInitial
        {
            get
            {
                string name = FTQuiz2_CurrentUser.Username;
                return string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
            }
        }

        /// <summary>The board search box only makes sense on Home (source: IsCurrentPage Home.aspx).</summary>
        public bool ShowSearch
        {
            get { return IsHome(); }
        }

        /// <summary>" active" for the Home rail link, otherwise "".</summary>
        public string HomeActiveClass()
        {
            return IsHome() ? " active" : string.Empty;
        }

        /// <summary>
        /// " active" when the current request is served by the given controller (My Account and the
        /// admin pages). Matching the controller, not the URL, keeps the link active on that page's
        /// POST actions too, like a Web Forms postback.
        /// </summary>
        public string ActiveClass(string controllerName)
        {
            return string.Equals(RouteValue("controller"), controllerName, StringComparison.OrdinalIgnoreCase)
                ? " active"
                : string.Empty;
        }

        private static bool IsHome()
        {
            string controller = RouteValue("controller");
            bool homeController = string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase)
                || string.Equals(controller, "FTQuiz2_Home", StringComparison.OrdinalIgnoreCase);
            return homeController
                && string.Equals(RouteValue("action"), "Index", StringComparison.OrdinalIgnoreCase);
        }

        private static string RouteValue(string key)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Request == null || ctx.Request.RequestContext == null) { return string.Empty; }
            return Convert.ToString(ctx.Request.RequestContext.RouteData.Values[key]) ?? string.Empty;
        }

        /// <summary>First letter of the first and last words, e.g. "Mark Rafael L. Guades" gives "MG".</summary>
        private static string GetInitials(string name)
        {
            string[] words = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) { return "P"; }

            string first = words[0].Substring(0, 1);
            string last = words.Length > 1 ? words[words.Length - 1].Substring(0, 1) : string.Empty;
            return (first + last).ToUpperInvariant();
        }

        /// <summary>Shows an 11-digit number as 0924 016 9431. Anything else is shown as stored.</summary>
        private static string FormatPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) { return string.Empty; }

            if (Regex.IsMatch(phone, @"^\d{11}$"))
            {
                return phone.Substring(0, 4) + " " + phone.Substring(4, 3) + " " + phone.Substring(7);
            }
            return phone;
        }
    }
}
