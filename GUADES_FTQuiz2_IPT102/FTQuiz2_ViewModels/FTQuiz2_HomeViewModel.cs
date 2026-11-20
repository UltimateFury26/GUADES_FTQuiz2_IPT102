using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>A published product as the homepage draws it: card plus its detail modal.</summary>
    public class FTQuiz2_ProductCardView
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Flavor { get; set; }              // empty when the product has no flavor
        public string ShortDescription { get; set; }    // empty when there is none
        public string FullDescription { get; set; }     // empty when there is none
        public string ImageUrl { get; set; }            // empty when there is no image on disk
        public string BackgroundColor { get; set; }     // safe "#RRGGBB" chosen by the admin; empty = automatic

        public bool HasBackground { get { return !string.IsNullOrEmpty(BackgroundColor); } }

        /// <summary>True when the chosen background is dark enough that the slide needs light text.</summary>
        public bool IsDarkBackground
        {
            get { return HasBackground && GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels.FTQuiz2_BusinessFormat.IsDarkColor(BackgroundColor); }
        }
        public string ModalId { get { return "productModal" + ProductId; } }

        /// <summary>One letter for the picture placeholder when the product has no image.</summary>
        public string PlaceholderLetter
        {
            get
            {
                string source = !string.IsNullOrWhiteSpace(Flavor) ? Flavor : Name;
                return string.IsNullOrWhiteSpace(source) ? "M" : source.Trim().Substring(0, 1).ToUpperInvariant();
            }
        }
    }

    /// <summary>
    /// Everything the Mogu Mogu homepage shows. The controller fills it from SQL Server
    /// (business profile, contact information, published products, latest news); the view only draws it.
    /// </summary>
    public class FTQuiz2_HomeViewModel
    {
        /// <summary>How many published products the homepage features (first ones in display order).</summary>
        public const int FeaturedProductLimit = 6;

        private const int StoryPreviewLength = 320;

        // ---- business profile (usp_BusinessProfile_Get) ----
        public string BusinessName { get; set; }
        public string Tagline { get; set; }
        public string BrandStory { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public string HeroUrl { get; set; }
        public string AboutImageUrl { get; set; }

        // ---- products (usp_Product_GetPublished) ----
        public List<FTQuiz2_ProductCardView> FeaturedProducts { get; set; }
        public int PublishedProductCount { get; set; }

        // ---- latest news (usp_Post_GetPublished) ----
        public FTQuiz2_PostView LatestPost { get; set; }
        public string LatestPostDate { get; set; }

        // ---- contact information (usp_ContactInfo_Get) ----
        public string Email { get; set; }
        public string PhoneDisplay { get; set; }
        public string PhoneHref { get; set; }           // digits and + only, for tel:
        public string Address { get; set; }
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }

        public FTQuiz2_HomeViewModel()
        {
            BusinessName = "Mogu Mogu";
            Tagline = string.Empty;
            BrandStory = string.Empty;
            Mission = string.Empty;
            Vision = string.Empty;
            HeroUrl = string.Empty;
            AboutImageUrl = string.Empty;
            FeaturedProducts = new List<FTQuiz2_ProductCardView>();
            LatestPostDate = string.Empty;
            Email = string.Empty;
            PhoneDisplay = string.Empty;
            PhoneHref = string.Empty;
            Address = string.Empty;
            FacebookUrl = string.Empty;
            InstagramUrl = string.Empty;
        }

        public string HeadlineText
        {
            get { return Tagline.Length > 0 ? Tagline : BusinessName; }
        }

        /// <summary>Short excerpt of the brand story for the homepage (full story: About page).</summary>
        public string StoryPreview
        {
            get { return Excerpt(BrandStory, StoryPreviewLength); }
        }

        public bool HasHighlights
        {
            get { return Mission.Length > 0 || Vision.Length > 0; }
        }

        public bool HasContact
        {
            get
            {
                return Email.Length > 0 || PhoneDisplay.Length > 0 || Address.Length > 0
                    || FacebookUrl.Length > 0 || InstagramUrl.Length > 0;
            }
        }

        public string FormatDate(DateTime date)
        {
            return date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>Phone number reduced to digits and a leading +, safe for a tel: link.</summary>
        public static string ToPhoneHref(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) { return string.Empty; }
            return new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        }

        /// <summary>Cuts text at a word boundary and adds an ellipsis; short text is returned unchanged.</summary>
        public static string Excerpt(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) { return string.Empty; }

            string clean = text.Trim();
            if (clean.Length <= maxLength) { return clean; }

            int cut = clean.LastIndexOf(' ', maxLength);
            if (cut < maxLength / 2) { cut = maxLength; }
            return clean.Substring(0, cut).TrimEnd(' ', ',', '.', ';', ':') + "…";
        }
    }
}
