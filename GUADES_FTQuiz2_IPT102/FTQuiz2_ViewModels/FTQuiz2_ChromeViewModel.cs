using System;
using System.Text.RegularExpressions;
using System.Web;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// Everything the shared shell needs: the business name and contact details from the database,
    /// the logged-in account, and which header link is active. Loaded at most once per request
    /// (ForRequest) and only by the signed-in layout, so Login/Signup/Error never touch the database
    /// for chrome.
    /// </summary>
    public class FTQuiz2_ChromeViewModel
    {
        private const string ItemsKey = "FTQuiz2_ChromeViewModel";

        // Defaults, so none of these is ever null.
        public string OwnerName { get; private set; }      // the business name
        public string Email { get; private set; }
        public string PhoneDisplay { get; private set; }
        public string Address { get; private set; }

        public string AccountName { get { return FTQuiz2_CurrentUser.Username; } }
        public string AccountRole { get { return FTQuiz2_CurrentUser.Role; } }
        public bool IsAdmin { get { return FTQuiz2_CurrentUser.IsAdmin; } }

        private FTQuiz2_ChromeViewModel()
        {
            OwnerName = "Mogu Mogu";
            Email = string.Empty;
            PhoneDisplay = string.Empty;
            Address = string.Empty;
        }

        /// <summary>The shared model for this request (loaded on first use).</summary>
        public static FTQuiz2_ChromeViewModel ForRequest()
        {
            HttpContext ctx = HttpContext.Current;
            FTQuiz2_ChromeViewModel model = ctx != null ? ctx.Items[ItemsKey] as FTQuiz2_ChromeViewModel : null;
            if (model != null) { return model; }

            using (var db = new FTQuiz2_Db())
            {
                var business = new FTQuiz2_BusinessRepository(db);
                model = From(business.GetBusinessProfile(), business.GetContactInfo());
            }

            if (ctx != null) { ctx.Items[ItemsKey] = model; }
            return model;
        }

        /// <summary>A missing profile or contact row keeps the defaults.</summary>
        public static FTQuiz2_ChromeViewModel From(FTQuiz2_BusinessProfile profile, FTQuiz2_ContactInfo contact)
        {
            FTQuiz2_ChromeViewModel model = new FTQuiz2_ChromeViewModel();

            if (profile != null && !string.IsNullOrWhiteSpace(profile.BusinessName))
            {
                model.OwnerName = profile.BusinessName;
            }

            if (contact != null)
            {
                model.Email = contact.Email ?? string.Empty;
                model.PhoneDisplay = FormatPhone(contact.Phone);
                model.Address = contact.Address ?? string.Empty;
            }
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

        /// <summary>" active" for the Home header link, otherwise "".</summary>
        public string HomeActiveClass()
        {
            return IsHome() ? " active" : string.Empty;
        }

        /// <summary>
        /// " active" when the current request is served by the given controller (My Account and the
        /// admin pages). Matching the controller, not the URL, keeps the link active on that page's
        /// POST actions too.
        /// </summary>
        public string ActiveClass(string controllerName)
        {
            return string.Equals(RouteValue("controller"), controllerName, StringComparison.OrdinalIgnoreCase)
                ? " active"
                : string.Empty;
        }

        /// <summary>
        /// " active" when the current request is the given controller AND action (Our Story and Contact
        /// are actions of the Home controller, so a controller-only match would also light up Home).
        /// </summary>
        public string ActiveClass(string controllerName, string actionName)
        {
            return string.Equals(RouteValue("controller"), controllerName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(RouteValue("action"), actionName, StringComparison.OrdinalIgnoreCase)
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
