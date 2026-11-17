using System;
using System.Collections.Generic;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    /// <summary>
    /// Row shape returned by usp_Post_GetPublished / GetAll / GetById.
    /// The procedures JOIN Users and return AuthorName (the source Post class has it);
    /// the FTQuiz2_Post entity has no such column, so this projection DTO carries it.
    /// </summary>
    public class FTQuiz2_PostView
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string AuthorName { get; set; }
    }

    /// <summary>One-for-one port of source Data/PostRepository.cs (7 methods), all on original procedures.</summary>
    public class FTQuiz2_PostRepository
    {
        private readonly FTQuiz2_Db _db;

        public FTQuiz2_PostRepository() : this(new FTQuiz2_Db()) { }
        public FTQuiz2_PostRepository(FTQuiz2_Db db) { _db = db; }

        // Newest first; only IsPublished = 1 (ordering/filter live in the procedure).
        public List<FTQuiz2_PostView> GetPublished()
        {
            return _db.Query<FTQuiz2_PostView>("usp_Post_GetPublished");
        }

        public List<FTQuiz2_PostView> GetAll()
        {
            return _db.Query<FTQuiz2_PostView>("usp_Post_GetAll");
        }

        // null when not found
        public FTQuiz2_PostView GetById(int postId)
        {
            return _db.QueryFirstOrDefault<FTQuiz2_PostView>("usp_Post_GetById",
                FTQuiz2_Db.P("@PostId", postId));
        }

        // Returns NewId. Procedure throws (SqlException) when title or body is blank.
        public int Insert(string title, string body, bool isPublished, int authorId)
        {
            return _db.QuerySingle<FTQuiz2_NewIdResult>("usp_Post_Insert",
                FTQuiz2_Db.P("@Title", title),
                FTQuiz2_Db.P("@Body", body),
                FTQuiz2_Db.P("@IsPublished", isPublished),
                FTQuiz2_Db.P("@AuthorId", authorId)).NewId;
        }

        // Returns rows affected. Procedure throws (SqlException) when title or body is blank.
        public int Update(int postId, string title, string body, bool isPublished)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Post_Update",
                FTQuiz2_Db.P("@PostId", postId),
                FTQuiz2_Db.P("@Title", title),
                FTQuiz2_Db.P("@Body", body),
                FTQuiz2_Db.P("@IsPublished", isPublished)).Affected;
        }

        public int SetPublished(int postId, bool isPublished)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Post_SetPublished",
                FTQuiz2_Db.P("@PostId", postId),
                FTQuiz2_Db.P("@IsPublished", isPublished)).Affected;
        }

        public int Delete(int postId)
        {
            return _db.QuerySingle<FTQuiz2_AffectedResult>("usp_Post_Delete",
                FTQuiz2_Db.P("@PostId", postId)).Affected;
        }
    }
}
