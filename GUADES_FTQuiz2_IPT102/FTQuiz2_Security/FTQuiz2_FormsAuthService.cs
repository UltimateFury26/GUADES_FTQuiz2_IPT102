using System;
using System.Web;
using System.Web.Security;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Security
{
    /// <summary>
    /// One place that issues and clears the Forms Authentication ticket (ADR-003).
    /// Cookie name (.PORTFOLIOAUTH), timeout, sliding expiration, protection and
    /// requireSSL all come from Web.config &lt;forms&gt; (set in Phase 12); nothing is hard-coded here.
    /// </summary>
    public static class FTQuiz2_FormsAuthService
    {
        /// <summary>
        /// Issues the ticket cookie. UserData = "UserId|Role". The source login never used a
        /// persistent cookie, so persist defaults to false.
        /// </summary>
        public static void SignIn(HttpContextBase http, int userId, string username, string role, bool persist = false)
        {
            if (http == null) { throw new ArgumentNullException("http"); }

            DateTime issued = DateTime.Now;

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                username,
                issued,
                issued.Add(FormsAuthentication.Timeout),
                persist,
                FTQuiz2_CurrentUser.BuildUserData(userId, role),
                FormsAuthentication.FormsCookiePath);

            HttpCookie cookie = new HttpCookie(
                FormsAuthentication.FormsCookieName,
                FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (persist) { cookie.Expires = ticket.Expiration; }

            http.Response.Cookies.Add(cookie);
        }

        /// <summary>Clears the ticket and ends the session (source: SignOut + Session.Abandon).</summary>
        public static void SignOut(HttpContextBase http)
        {
            if (http == null) { throw new ArgumentNullException("http"); }

            FormsAuthentication.SignOut();

            if (http.Session != null) { http.Session.Abandon(); }
        }
    }
}
