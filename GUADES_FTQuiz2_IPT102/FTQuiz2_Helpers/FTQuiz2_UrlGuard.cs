using System;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers
{
    /// <summary>Port of source Helpers/UrlHelper (admin-typed addresses shown as links).</summary>
    public static class FTQuiz2_UrlGuard
    {
        /// <summary>
        /// Returns the address (trimmed, otherwise unchanged) only when it is an absolute
        /// http or https URL; otherwise "". "javascript:..." can never become a link.
        /// </summary>
        public static string SafeHttpUrl(string url)
        {
            Uri uri;
            if (!string.IsNullOrWhiteSpace(url)
                && Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return url.Trim();
            }

            return string.Empty;
        }
    }
}
