using System;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;

namespace GUADES_FTQuiz2_IPT102
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // Port of Global.Application_PostAuthenticateRequest: rebuild the principal with the
        // role from the ticket's "UserId|Role" UserData so User.IsInRole(...) works.
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            FormsIdentity identity = Context.User != null
                ? Context.User.Identity as FormsIdentity
                : null;

            if (identity == null) { return; }

            int userId;
            string role;
            if (FTQuiz2_CurrentUser.TryParseUserData(identity.Ticket.UserData, out userId, out role))
            {
                Context.User = new GenericPrincipal(identity, new[] { role });
            }
        }

        // Port of Global.Application_Error: append to App_Data, never throw.
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex == null) { return; }

            try
            {
                string logPath = Server.MapPath("~/App_Data/FTQuiz2_error.log");
                string entry = string.Format(
                    "{0:u} | {1} | {2}{3}{4}{5}",
                    DateTime.UtcNow,
                    Request != null ? Request.RawUrl : "(no request)",
                    ex.GetType().Name,
                    Environment.NewLine,
                    ex.ToString(),
                    Environment.NewLine + "----" + Environment.NewLine);

                File.AppendAllText(logPath, entry);
            }
            catch
            {
                // logging must never throw
            }
        }
    }
}
