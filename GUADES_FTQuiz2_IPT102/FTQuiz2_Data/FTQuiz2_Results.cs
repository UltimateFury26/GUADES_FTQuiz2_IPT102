namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    // Property names match the procedure result-set column names exactly,
    // because Database.SqlQuery<T> binds by column name.

    /// <summary>usp_User_Register: 0 OK | 1 username taken | 2 email taken | 3 invalid input.</summary>
    public class FTQuiz2_RegisterResult
    {
        public int ResultCode { get; set; }
        public int? UserId { get; set; }
    }

    /// <summary>usp_User_Login: 0 OK | 1 invalid credentials | 2 inactive.</summary>
    public class FTQuiz2_LoginResult
    {
        public int ResultCode { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }

    /// <summary>Any procedure whose result set is a single ResultCode column.</summary>
    public class FTQuiz2_CodeResult
    {
        public int ResultCode { get; set; }
    }

    /// <summary>Content UPDATE / DELETE procedures: rows affected.</summary>
    public class FTQuiz2_AffectedResult
    {
        public int Affected { get; set; }
    }

    /// <summary>Content INSERT procedures: the new identity value.</summary>
    public class FTQuiz2_NewIdResult
    {
        public int NewId { get; set; }
    }
}
