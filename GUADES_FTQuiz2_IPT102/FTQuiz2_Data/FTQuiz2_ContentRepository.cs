using System.Collections.Generic;
using System.Data.SqlClient;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    /// <summary>
    /// One-for-one port of source Data/ContentRepository.cs (27 methods).
    /// Every method stays on its original stored procedure (ADR-001), so ordering,
    /// NULL = "keep current" rules (image path / sort order on UPDATE), NewId and
    /// Affected semantics are all decided by the procedures exactly as before.
    ///
    /// Pass null (never "") for an image path when no new file was uploaded.
    /// sortOrder null = "place last" on insert, "keep current" on update.
    /// </summary>
    public class FTQuiz2_ContentRepository
    {
        private readonly FTQuiz2_Db _db;

        public FTQuiz2_ContentRepository() : this(new FTQuiz2_Db()) { }
        public FTQuiz2_ContentRepository(FTQuiz2_Db db) { _db = db; }

        // ---- PersonalInfo (singleton row) ----------------------------------

        // null when the singleton row is missing (source: Rows.Count == 0 -> null)
        public FTQuiz2_PersonalInfo GetPersonalInfo()
        {
            return _db.QueryFirstOrDefault<FTQuiz2_PersonalInfo>("usp_PersonalInfo_Get");
        }

        // Procedure throws if FullName is blank; AvatarPath null = keep current avatar.
        public void UpdatePersonalInfo(FTQuiz2_PersonalInfo info)
        {
            _db.Execute("usp_PersonalInfo_Update",
                FTQuiz2_Db.P("@FullName", info.FullName),
                FTQuiz2_Db.P("@Tagline", info.Tagline),
                FTQuiz2_Db.P("@Bio", info.Bio),
                FTQuiz2_Db.P("@Email", info.Email),
                FTQuiz2_Db.P("@Phone", info.Phone),
                FTQuiz2_Db.P("@Address", info.Address),
                FTQuiz2_Db.P("@GitHubUrl", info.GitHubUrl),
                FTQuiz2_Db.P("@LinkedInUrl", info.LinkedInUrl),
                FTQuiz2_Db.P("@AvatarPath", info.AvatarPath));
        }

        // ---- Projects --------------------------------------------------------

        public List<FTQuiz2_Project> GetProjects()
        {
            return _db.Query<FTQuiz2_Project>("usp_Project_GetAll");
        }

        public FTQuiz2_Project GetProjectById(int projectId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_Project>("usp_Project_GetById",
                FTQuiz2_Db.P("@ProjectId", projectId));
        }

        public int InsertProject(FTQuiz2_Project project, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Project_Insert",
                ProjectParameters(project, false, sortOrder)).NewId;
        }

        public int UpdateProject(FTQuiz2_Project project, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Project_Update",
                ProjectParameters(project, true, sortOrder)).Affected;
        }

        public int DeleteProject(int projectId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Project_Delete",
                FTQuiz2_Db.P("@ProjectId", projectId)).Affected;
        }

        // ---- Skills ----------------------------------------------------------

        // Source passes @Category = NULL (every category).
        public List<FTQuiz2_Skill> GetSkills()
        {
            return _db.Query<FTQuiz2_Skill>("usp_Skill_GetAll",
                FTQuiz2_Db.P("@Category", null));
        }

        public FTQuiz2_Skill GetSkillById(int skillId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_Skill>("usp_Skill_GetById",
                FTQuiz2_Db.P("@SkillId", skillId));
        }

        public int InsertSkill(FTQuiz2_Skill skill, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Skill_Insert",
                SkillParameters(skill, false, sortOrder)).NewId;
        }

        public int UpdateSkill(FTQuiz2_Skill skill, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Skill_Update",
                SkillParameters(skill, true, sortOrder)).Affected;
        }

        public int DeleteSkill(int skillId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Skill_Delete",
                FTQuiz2_Db.P("@SkillId", skillId)).Affected;
        }

        // ---- Education -------------------------------------------------------

        public List<FTQuiz2_Education> GetEducation()
        {
            return _db.Query<FTQuiz2_Education>("usp_Education_GetAll");
        }

        public FTQuiz2_Education GetEducationById(int educationId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_Education>("usp_Education_GetById",
                FTQuiz2_Db.P("@EducationId", educationId));
        }

        public int InsertEducation(FTQuiz2_Education entry, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Education_Insert",
                EducationParameters(entry, false, sortOrder)).NewId;
        }

        public int UpdateEducation(FTQuiz2_Education entry, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Education_Update",
                EducationParameters(entry, true, sortOrder)).Affected;
        }

        public int DeleteEducation(int educationId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Education_Delete",
                FTQuiz2_Db.P("@EducationId", educationId)).Affected;
        }

        // ---- Gallery ---------------------------------------------------------

        public List<FTQuiz2_GalleryItem> GetGallery()
        {
            return _db.Query<FTQuiz2_GalleryItem>("usp_Gallery_GetAll");
        }

        public FTQuiz2_GalleryItem GetGalleryById(int galleryId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_GalleryItem>("usp_Gallery_GetById",
                FTQuiz2_Db.P("@GalleryId", galleryId));
        }

        public int InsertGalleryItem(FTQuiz2_GalleryItem entry, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Gallery_Insert",
                GalleryParameters(entry, false, sortOrder)).NewId;
        }

        public int UpdateGalleryItem(FTQuiz2_GalleryItem entry, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Gallery_Update",
                GalleryParameters(entry, true, sortOrder)).Affected;
        }

        public int DeleteGalleryItem(int galleryId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Gallery_Delete",
                FTQuiz2_Db.P("@GalleryId", galleryId)).Affected;
        }

        // ---- Hobbies ---------------------------------------------------------

        public List<FTQuiz2_Hobby> GetHobbies()
        {
            return _db.Query<FTQuiz2_Hobby>("usp_Hobby_GetAll");
        }

        public FTQuiz2_Hobby GetHobbyById(int hobbyId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_Hobby>("usp_Hobby_GetById",
                FTQuiz2_Db.P("@HobbyId", hobbyId));
        }

        public int InsertHobby(FTQuiz2_Hobby entry, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Hobby_Insert",
                HobbyParameters(entry, false, sortOrder)).NewId;
        }

        public int UpdateHobby(FTQuiz2_Hobby entry, int? sortOrder)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Hobby_Update",
                HobbyParameters(entry, true, sortOrder)).Affected;
        }

        public int DeleteHobby(int hobbyId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Hobby_Delete",
                FTQuiz2_Db.P("@HobbyId", hobbyId)).Affected;
        }

        // ---- parameter builders (names bind by name, so order is irrelevant) ----

        private static SqlParameter[] ProjectParameters(FTQuiz2_Project project, bool includeId, int? sortOrder)
        {
            List<SqlParameter> p = new List<SqlParameter>();
            if (includeId) { p.Add(FTQuiz2_Db.P("@ProjectId", project.ProjectId)); }
            p.Add(FTQuiz2_Db.P("@Title", project.Title));
            p.Add(FTQuiz2_Db.P("@TechStack", project.TechStack));
            p.Add(FTQuiz2_Db.P("@Role", project.Role));
            p.Add(FTQuiz2_Db.P("@Details", project.Details));
            p.Add(FTQuiz2_Db.P("@RepoUrl", project.RepoUrl));
            p.Add(FTQuiz2_Db.P("@SortOrder", sortOrder));
            return p.ToArray();
        }

        private static SqlParameter[] SkillParameters(FTQuiz2_Skill skill, bool includeId, int? sortOrder)
        {
            List<SqlParameter> p = new List<SqlParameter>();
            if (includeId) { p.Add(FTQuiz2_Db.P("@SkillId", skill.SkillId)); }
            p.Add(FTQuiz2_Db.P("@Name", skill.Name));
            p.Add(FTQuiz2_Db.P("@Category", skill.Category));
            p.Add(FTQuiz2_Db.P("@Description", skill.Description));
            p.Add(FTQuiz2_Db.P("@SortOrder", sortOrder));
            return p.ToArray();
        }

        private static SqlParameter[] EducationParameters(FTQuiz2_Education entry, bool includeId, int? sortOrder)
        {
            List<SqlParameter> p = new List<SqlParameter>();
            if (includeId) { p.Add(FTQuiz2_Db.P("@EducationId", entry.EducationId)); }
            p.Add(FTQuiz2_Db.P("@SchoolName", entry.SchoolName));
            p.Add(FTQuiz2_Db.P("@Level", entry.Level));
            p.Add(FTQuiz2_Db.P("@Program", entry.Program));
            p.Add(FTQuiz2_Db.P("@StartYear", entry.StartYear));
            p.Add(FTQuiz2_Db.P("@EndYear", entry.EndYear));
            p.Add(FTQuiz2_Db.P("@Description", entry.Description));
            p.Add(FTQuiz2_Db.P("@PhotoPath", entry.PhotoPath));
            p.Add(FTQuiz2_Db.P("@SortOrder", sortOrder));
            return p.ToArray();
        }

        private static SqlParameter[] GalleryParameters(FTQuiz2_GalleryItem entry, bool includeId, int? sortOrder)
        {
            List<SqlParameter> p = new List<SqlParameter>();
            if (includeId) { p.Add(FTQuiz2_Db.P("@GalleryId", entry.GalleryId)); }
            p.Add(FTQuiz2_Db.P("@ImagePath", entry.ImagePath));
            p.Add(FTQuiz2_Db.P("@Caption", entry.Caption));
            p.Add(FTQuiz2_Db.P("@SortOrder", sortOrder));
            return p.ToArray();
        }

        private static SqlParameter[] HobbyParameters(FTQuiz2_Hobby entry, bool includeId, int? sortOrder)
        {
            List<SqlParameter> p = new List<SqlParameter>();
            if (includeId) { p.Add(FTQuiz2_Db.P("@HobbyId", entry.HobbyId)); }
            p.Add(FTQuiz2_Db.P("@Title", entry.Title));
            p.Add(FTQuiz2_Db.P("@Description", entry.Description));
            p.Add(FTQuiz2_Db.P("@ImagePath", entry.ImagePath));
            p.Add(FTQuiz2_Db.P("@SortOrder", sortOrder));
            return p.ToArray();
        }
    }
}
