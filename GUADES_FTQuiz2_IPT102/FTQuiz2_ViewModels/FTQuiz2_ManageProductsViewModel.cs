using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// Manage Products page: the admin list (published and unpublished) and the add/edit form.
    /// Same class is the Razor model and the POST model of Save, like the Manage Content editors, so the
    /// field names stay plain ("ProductName") and the DataAnnotations also drive the browser-side checks.
    /// </summary>
    public class FTQuiz2_ManageProductsViewModel : FTQuiz2_EditorViewModelBase
    {
        public List<FTQuiz2_Product> Products { get; set; }

        // ---- form fields ----
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Enter the product name.")]
        [StringLength(FTQuiz2_ContentFormat.ProductNameMax, ErrorMessage = "The product name can be up to 100 characters.")]
        public string ProductName { get; set; }

        [StringLength(FTQuiz2_ContentFormat.FlavorMax, ErrorMessage = "The flavor can be up to 50 characters.")]
        public string Flavor { get; set; }

        [StringLength(FTQuiz2_ContentFormat.ShortDescriptionMax, ErrorMessage = "The short description can be up to 250 characters.")]
        public string ShortDescription { get; set; }

        [StringLength(FTQuiz2_ContentFormat.FullDescriptionMax, ErrorMessage = "The full description can be up to 4000 characters.")]
        public string FullDescription { get; set; }

        [RegularExpression(FTQuiz2_ContentFormat.SortOrderPattern, ErrorMessage = FTQuiz2_ContentFormat.SortOrderMessage)]
        public string SortOrder { get; set; }

        public bool IsPublished { get; set; }

        public FTQuiz2_ManageProductsViewModel()
        {
            Products = new List<FTQuiz2_Product>();
            IsPublished = true;
        }

        public string FormTitle { get { return IsEdit ? "Edit product" : "Add a product"; } }

        public string ImageHint
        {
            get
            {
                return IsEdit
                    ? "JPG, PNG or GIF, up to 5 MB. Leave empty to keep the current image."
                    : "JPG, PNG or GIF, up to 5 MB.";
            }
        }

        /// <summary>"4 products, 3 published"; empty when the list could not be loaded.</summary>
        public string CountText
        {
            get
            {
                if (LoadFailed) { return string.Empty; }

                int published = 0;
                foreach (FTQuiz2_Product product in Products)
                {
                    if (product.IsPublished) { published++; }
                }

                string noun = Products.Count == 1 ? "1 product" : Products.Count + " products";
                return noun + ", " + published + " published";
            }
        }

        public string StatusText(bool isPublished) { return isPublished ? "Published" : "Draft"; }
        public string StatusBadgeClass(bool isPublished) { return isPublished ? "badge badge-active" : "badge badge-user"; }
    }
}
