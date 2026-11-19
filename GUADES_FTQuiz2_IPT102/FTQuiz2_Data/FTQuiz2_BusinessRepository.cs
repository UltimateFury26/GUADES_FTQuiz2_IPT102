using System.Collections.Generic;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    /// <summary>
    /// Business profile (About), contact information and owners, all on stored procedures
    /// through FTQuiz2_Db (ADR-001), same pattern as FTQuiz2_ContentRepository.
    ///
    /// Pass null (never "") for an image path when no new file was uploaded: the procedure keeps the
    /// current image. sortOrder null = "place last" on insert, "keep current" on update.
    /// </summary>
    public class FTQuiz2_BusinessRepository
    {
        private readonly FTQuiz2_Db _db;

        public FTQuiz2_BusinessRepository() : this(new FTQuiz2_Db()) { }
        public FTQuiz2_BusinessRepository(FTQuiz2_Db db) { _db = db; }

        // ---- BusinessProfile (singleton row) --------------------------------

        /// <summary>Null when the singleton row is missing.</summary>
        public FTQuiz2_BusinessProfile GetBusinessProfile()
        {
            return _db.QueryFirstOrDefault<FTQuiz2_BusinessProfile>("usp_BusinessProfile_Get");
        }

        /// <summary>The procedure throws when BusinessName is blank. Null image paths keep the current images.</summary>
        public void UpdateBusinessProfile(FTQuiz2_BusinessProfile profile)
        {
            _db.Execute("usp_BusinessProfile_Update",
                FTQuiz2_Db.P("@BusinessName", profile.BusinessName),
                FTQuiz2_Db.P("@Tagline", profile.Tagline),
                FTQuiz2_Db.P("@BrandStory", profile.BrandStory),
                FTQuiz2_Db.P("@Mission", profile.Mission),
                FTQuiz2_Db.P("@Vision", profile.Vision),
                FTQuiz2_Db.P("@AboutImagePath", profile.AboutImagePath),
                FTQuiz2_Db.P("@HeroImagePath", profile.HeroImagePath));
        }

        // ---- ContactInfo (singleton row) ------------------------------------

        /// <summary>Null when the singleton row is missing.</summary>
        public FTQuiz2_ContactInfo GetContactInfo()
        {
            return _db.QueryFirstOrDefault<FTQuiz2_ContactInfo>("usp_ContactInfo_Get");
        }

        /// <summary>The procedure throws when a non-blank email is not valid.</summary>
        public void UpdateContactInfo(FTQuiz2_ContactInfo contact)
        {
            _db.Execute("usp_ContactInfo_Update",
                FTQuiz2_Db.P("@Email", contact.Email),
                FTQuiz2_Db.P("@Phone", contact.Phone),
                FTQuiz2_Db.P("@Address", contact.Address),
                FTQuiz2_Db.P("@FacebookUrl", contact.FacebookUrl),
                FTQuiz2_Db.P("@InstagramUrl", contact.InstagramUrl));
        }

        // ---- Owners ----------------------------------------------------------

        public List<FTQuiz2_OwnerInfo> GetOwners()
        {
            return _db.Query<FTQuiz2_OwnerInfo>("usp_Owner_GetAll");
        }

        public FTQuiz2_OwnerInfo GetOwnerById(int ownerId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_OwnerInfo>("usp_Owner_GetById",
                FTQuiz2_Db.P("@OwnerId", ownerId));
        }

        public int InsertOwner(FTQuiz2_OwnerInfo owner, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Owner_Insert",
                FTQuiz2_Db.P("@FullName", owner.FullName),
                FTQuiz2_Db.P("@Position", owner.Position),
                FTQuiz2_Db.P("@Bio", owner.Bio),
                FTQuiz2_Db.P("@Email", owner.Email),
                FTQuiz2_Db.P("@PhotoPath", owner.PhotoPath),
                FTQuiz2_Db.P("@SortOrder", sortOrder)).NewId;
        }

        /// <summary>Returns rows affected (0 = the owner no longer exists).</summary>
        public int UpdateOwner(FTQuiz2_OwnerInfo owner, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Owner_Update",
                FTQuiz2_Db.P("@OwnerId", owner.OwnerId),
                FTQuiz2_Db.P("@FullName", owner.FullName),
                FTQuiz2_Db.P("@Position", owner.Position),
                FTQuiz2_Db.P("@Bio", owner.Bio),
                FTQuiz2_Db.P("@Email", owner.Email),
                FTQuiz2_Db.P("@PhotoPath", owner.PhotoPath),
                FTQuiz2_Db.P("@SortOrder", sortOrder)).Affected;
        }

        public int DeleteOwner(int ownerId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Owner_Delete",
                FTQuiz2_Db.P("@OwnerId", ownerId)).Affected;
        }
    }
}
