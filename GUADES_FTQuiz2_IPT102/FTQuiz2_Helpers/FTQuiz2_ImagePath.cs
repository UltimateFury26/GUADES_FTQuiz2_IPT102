using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers
{
    /// <summary>
    /// Port of source Helpers/ImageHelper.ResolveExisting. Uses HostingEnvironment.MapPath
    /// (no HttpContext needed) so it works from controllers, views and view models.
    /// </summary>
    public static class FTQuiz2_ImagePath
    {
        /// <summary>
        /// Turns a stored relative path (e.g. "FTQuiz2_Media/headshot.jpg") into a site-absolute
        /// URL, only when the path stays inside the site and the file exists. Otherwise "",
        /// so a missing image shows the icon cover instead of a broken picture.
        /// </summary>
        public static string Resolve(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) { return string.Empty; }

            string clean = relativePath.Trim().Replace('\\', '/').TrimStart('~', '/');

            if (clean.Length == 0 || clean.Contains("..") || clean.Contains(":")) { return string.Empty; }

            try
            {
                string physical = HostingEnvironment.MapPath("~/" + clean);
                return (physical != null && File.Exists(physical))
                    ? VirtualPathUtility.ToAbsolute("~/" + clean)
                    : string.Empty;
            }
            catch (HttpException)
            {
                return string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }
    }
}
