using System.Collections.Generic;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    /// <summary>
    /// Product catalogue on stored procedures through FTQuiz2_Db (ADR-001).
    /// Public pages call GetPublishedProducts / GetPublishedProductById; the admin pages call GetProducts
    /// (published and unpublished). Pass null (never "") for imagePath when no new file was uploaded.
    /// </summary>
    public class FTQuiz2_ProductRepository
    {
        private readonly FTQuiz2_Db _db;

        public FTQuiz2_ProductRepository() : this(new FTQuiz2_Db()) { }
        public FTQuiz2_ProductRepository(FTQuiz2_Db db) { _db = db; }

        /// <summary>Admin list: every product, in display order.</summary>
        public List<FTQuiz2_Product> GetProducts()
        {
            return _db.Query<FTQuiz2_Product>("usp_Product_GetAll");
        }

        /// <summary>Public catalogue: published products only, in display order.</summary>
        public List<FTQuiz2_Product> GetPublishedProducts()
        {
            return _db.Query<FTQuiz2_Product>("usp_Product_GetPublished");
        }

        /// <summary>Admin lookup: any product, or null.</summary>
        public FTQuiz2_Product GetProductById(int productId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_Product>("usp_Product_GetById",
                FTQuiz2_Db.P("@ProductId", productId));
        }

        /// <summary>Public lookup: the product only when it is published (an unpublished id gives null).</summary>
        public FTQuiz2_Product GetPublishedProductById(int productId)
        {
            FTQuiz2_Product product = GetProductById(productId);
            return (product != null && product.IsPublished) ? product : null;
        }

        public int InsertProduct(FTQuiz2_Product product, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Product_Insert",
                FTQuiz2_Db.P("@ProductName", product.ProductName),
                FTQuiz2_Db.P("@Flavor", product.Flavor),
                FTQuiz2_Db.P("@ShortDescription", product.ShortDescription),
                FTQuiz2_Db.P("@FullDescription", product.FullDescription),
                FTQuiz2_Db.P("@ImagePath", product.ImagePath),
                FTQuiz2_Db.P("@SortOrder", sortOrder),
                FTQuiz2_Db.P("@IsPublished", product.IsPublished)).NewId;
        }

        /// <summary>Returns rows affected (0 = the product no longer exists). Status and order are updated too.</summary>
        public int UpdateProduct(FTQuiz2_Product product, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Product_Update",
                FTQuiz2_Db.P("@ProductId", product.ProductId),
                FTQuiz2_Db.P("@ProductName", product.ProductName),
                FTQuiz2_Db.P("@Flavor", product.Flavor),
                FTQuiz2_Db.P("@ShortDescription", product.ShortDescription),
                FTQuiz2_Db.P("@FullDescription", product.FullDescription),
                FTQuiz2_Db.P("@ImagePath", product.ImagePath),
                FTQuiz2_Db.P("@SortOrder", sortOrder),
                FTQuiz2_Db.P("@IsPublished", product.IsPublished)).Affected;
        }

        public int SetPublished(int productId, bool isPublished)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Product_SetPublished",
                FTQuiz2_Db.P("@ProductId", productId),
                FTQuiz2_Db.P("@IsPublished", isPublished)).Affected;
        }

        public int SetSortOrder(int productId, int sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Product_SetSortOrder",
                FTQuiz2_Db.P("@ProductId", productId),
                FTQuiz2_Db.P("@SortOrder", sortOrder)).Affected;
        }

        public int DeleteProduct(int productId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Product_Delete",
                FTQuiz2_Db.P("@ProductId", productId)).Affected;
        }
    }
}
