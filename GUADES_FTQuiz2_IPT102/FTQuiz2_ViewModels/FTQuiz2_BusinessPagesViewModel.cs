using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// Small formatting rules shared by the business pages (About, Products, Contact, Owners).
    /// Everything here is about showing database values safely: nothing is invented and nothing unsafe is rendered.
    /// </summary>
    public static class FTQuiz2_BusinessFormat
    {
        // No quotes, commas, colons, query characters or spaces: a value that passes cannot add a second
        // recipient, a subject or a body to a mailto: link.
        private static readonly Regex SafeEmailPattern =
            new Regex(@"^[^@\s<>""',;:?&]+@[^@\s<>""',;:?&]+\.[^@\s<>""',;:?&]+$", RegexOptions.Compiled);

        /// <summary>The email when it is a plain address safe for a mailto: link, otherwise "".</summary>
        public static string SafeEmail(string email)
        {
            string value = (email ?? string.Empty).Trim();
            return SafeEmailPattern.IsMatch(value) ? value : string.Empty;
        }

        /// <summary>
        /// True for text the seed data marks as an editable placeholder, such as "[Owner name - to be completed]".
        /// Such text is shown as stored, with a visible "placeholder" label, never presented as a verified fact.
        /// </summary>
        public static bool IsPlaceholder(string text)
        {
            string value = (text ?? string.Empty).Trim();
            return value.Length > 2 && value[0] == '[' && value[value.Length - 1] == ']';
        }

        /// <summary>A link for display: no scheme and no trailing slash ("facebook.com/mogumogu").</summary>
        public static string DisplayUrl(string url)
        {
            string value = (url ?? string.Empty).Trim();
            int scheme = value.IndexOf("://", StringComparison.Ordinal);
            if (scheme >= 0) { value = value.Substring(scheme + 3); }
            return value.TrimEnd('/');
        }

        /// <summary>First letter of a name for a picture placeholder ("M" when there is none).</summary>
        public static string Initial(string name)
        {
            string value = (name ?? string.Empty).Trim().TrimStart('[', ' ');
            return value.Length == 0 ? "M" : value.Substring(0, 1).ToUpperInvariant();
        }

        /// <summary>Turns a product row into the card the pages draw (the same card the homepage uses).</summary>
        public static FTQuiz2_ProductCardView ToCard(FTQuiz2_Product product)
        {
            return new FTQuiz2_ProductCardView
            {
                ProductId = product.ProductId,
                Name = product.ProductName,
                Flavor = product.Flavor ?? string.Empty,
                ShortDescription = product.ShortDescription ?? string.Empty,
                FullDescription = product.FullDescription ?? string.Empty,
                ImageUrl = FTQuiz2_ImagePath.Resolve(product.ImagePath)
            };
        }
    }

    /// <summary>Our Story page: the business profile record (usp_BusinessProfile_Get).</summary>
    public class FTQuiz2_AboutViewModel
    {
        public string BusinessName { get; set; }
        public string Tagline { get; set; }
        public string BrandStory { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }

        /// <summary>The About picture, or the hero picture when no About picture is set; empty when neither exists on disk.</summary>
        public string ImageUrl { get; set; }

        public FTQuiz2_AboutViewModel()
        {
            BusinessName = "Mogu Mogu";
            Tagline = string.Empty;
            BrandStory = string.Empty;
            Mission = string.Empty;
            Vision = string.Empty;
            ImageUrl = string.Empty;
        }

        public bool HasStory { get { return BrandStory.Length > 0; } }
        public bool StoryIsPlaceholder { get { return FTQuiz2_BusinessFormat.IsPlaceholder(BrandStory); } }
        public bool HasHighlights { get { return Mission.Length > 0 || Vision.Length > 0; } }
    }

    /// <summary>Products page: published products only, in display order.</summary>
    public class FTQuiz2_ProductsIndexViewModel
    {
        public string BusinessName { get; set; }
        public List<FTQuiz2_ProductCardView> Products { get; set; }

        public FTQuiz2_ProductsIndexViewModel()
        {
            BusinessName = "Mogu Mogu";
            Products = new List<FTQuiz2_ProductCardView>();
        }
    }

    /// <summary>One published product, plus a few other published products to browse next.</summary>
    public class FTQuiz2_ProductDetailsViewModel
    {
        public const int OtherProductLimit = 3;

        public string BusinessName { get; set; }
        public FTQuiz2_ProductCardView Product { get; set; }
        public List<FTQuiz2_ProductCardView> OtherProducts { get; set; }

        public FTQuiz2_ProductDetailsViewModel()
        {
            BusinessName = "Mogu Mogu";
            OtherProducts = new List<FTQuiz2_ProductCardView>();
        }

        /// <summary>The long description when there is one, otherwise the short one; empty when neither exists.</summary>
        public string Description
        {
            get
            {
                return Product.FullDescription.Length > 0 ? Product.FullDescription : Product.ShortDescription;
            }
        }
    }

    /// <summary>Contact page: the contact information record (usp_ContactInfo_Get). Empty values are not drawn.</summary>
    public class FTQuiz2_ContactViewModel
    {
        public string BusinessName { get; set; }
        public string Email { get; set; }
        public string PhoneDisplay { get; set; }
        public string PhoneHref { get; set; }       // digits and + only, for tel:
        public string Address { get; set; }
        public string FacebookUrl { get; set; }     // empty unless a real http/https address
        public string InstagramUrl { get; set; }

        public FTQuiz2_ContactViewModel()
        {
            BusinessName = "Mogu Mogu";
            Email = string.Empty;
            PhoneDisplay = string.Empty;
            PhoneHref = string.Empty;
            Address = string.Empty;
            FacebookUrl = string.Empty;
            InstagramUrl = string.Empty;
        }

        public bool HasContact
        {
            get
            {
                return Email.Length > 0 || PhoneDisplay.Length > 0 || Address.Length > 0
                    || FacebookUrl.Length > 0 || InstagramUrl.Length > 0;
            }
        }

        public bool HasLinks { get { return FacebookUrl.Length > 0 || InstagramUrl.Length > 0; } }
    }

    /// <summary>One owner / company representative as the Owners page draws them.</summary>
    public class FTQuiz2_OwnerCardView
    {
        public int OwnerId { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }        // empty when none
        public string Bio { get; set; }             // empty when none
        public string Email { get; set; }           // empty unless a safe address
        public string PhotoUrl { get; set; }        // empty when there is no photo on disk
        public bool IsPlaceholder { get; set; }     // the record still holds the seed's "to be completed" text

        public string Initial { get { return FTQuiz2_BusinessFormat.Initial(FullName); } }
    }

    /// <summary>
    /// About Us page: the contact information record plus the owner records, on one page.
    /// This replaces the separate Contact and Owners / Company pages, the way the real Mogu Mogu
    /// site keeps its story, people and contact details together.
    /// </summary>
    public class FTQuiz2_AboutUsViewModel
    {
        public string BusinessName { get; set; }
        public string Tagline { get; set; }
        public string BrandStory { get; set; }
        public string HeroImageUrl { get; set; }

        public bool HasBrandStory { get { return BrandStory.Length > 0 && !FTQuiz2_BusinessFormat.IsPlaceholder(BrandStory); } }

        // ---- contact information (usp_ContactInfo_Get) ----
        public string Email { get; set; }
        public string PhoneDisplay { get; set; }
        public string PhoneHref { get; set; }       // digits and + only, for tel:
        public string Address { get; set; }
        public string FacebookUrl { get; set; }     // empty unless a real http/https address
        public string InstagramUrl { get; set; }

        // ---- owners (usp_Owner_GetAll) ----
        public List<FTQuiz2_OwnerCardView> Owners { get; set; }

        public FTQuiz2_AboutUsViewModel()
        {
            BusinessName = "Mogu Mogu";
            Tagline = string.Empty;
            BrandStory = string.Empty;
            HeroImageUrl = string.Empty;
            Email = string.Empty;
            PhoneDisplay = string.Empty;
            PhoneHref = string.Empty;
            Address = string.Empty;
            FacebookUrl = string.Empty;
            InstagramUrl = string.Empty;
            Owners = new List<FTQuiz2_OwnerCardView>();
        }

        public bool HasContact
        {
            get
            {
                return Email.Length > 0 || PhoneDisplay.Length > 0 || Address.Length > 0
                    || FacebookUrl.Length > 0 || InstagramUrl.Length > 0;
            }
        }

        public bool HasOwners { get { return Owners.Count > 0; } }

        public bool HasPlaceholders
        {
            get
            {
                foreach (FTQuiz2_OwnerCardView owner in Owners)
                {
                    if (owner.IsPlaceholder) { return true; }
                }
                return false;
            }
        }
    }

    /// <summary>Owners / Company page: owner records (usp_Owner_GetAll), in display order.</summary>
    public class FTQuiz2_OwnersViewModel
    {
        public string BusinessName { get; set; }
        public List<FTQuiz2_OwnerCardView> Owners { get; set; }

        public FTQuiz2_OwnersViewModel()
        {
            BusinessName = "Mogu Mogu";
            Owners = new List<FTQuiz2_OwnerCardView>();
        }

        public bool HasPlaceholders
        {
            get
            {
                foreach (FTQuiz2_OwnerCardView owner in Owners)
                {
                    if (owner.IsPlaceholder) { return true; }
                }
                return false;
            }
        }
    }
}
