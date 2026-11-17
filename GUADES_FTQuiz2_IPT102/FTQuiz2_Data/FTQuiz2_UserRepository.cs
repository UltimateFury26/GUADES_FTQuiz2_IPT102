using System;
using System.Collections.Generic;
using System.Linq;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    /// <summary>
    /// The columns usp_User_GetById and usp_Admin_GetUsers actually return. EF's SqlQuery requires the
    /// result set to contain every property of the target type, and these procedures deliberately do
    /// NOT return PasswordHash / PasswordSalt, so they cannot be read straight into FTQuiz2_User.
    /// </summary>
    internal class FTQuiz2_UserRow
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public FTQuiz2_User ToUser()
        {
            return new FTQuiz2_User
            {
                UserId = UserId, Username = Username, Email = Email, Phone = Phone,
                Role = Role, Status = Status, CreatedAt = CreatedAt, LastLoginAt = LastLoginAt
            };
        }
    }

    /// <summary>
    /// One-for-one port of source Data/UserRepository.cs (9 methods).
    /// Every method stays on its original stored procedure (ADR-001).
    /// Passwords are only ever passed in as parameters; they are never logged
    /// and no hash/salt is read here (the procedures do not return them).
    /// </summary>
    public class FTQuiz2_UserRepository
    {
        private readonly FTQuiz2_Db _db;

        public FTQuiz2_UserRepository() : this(new FTQuiz2_Db()) { }
        public FTQuiz2_UserRepository(FTQuiz2_Db db) { _db = db; }

        // 0 = OK, 1 = username taken, 2 = email taken, 3 = invalid input
        public FTQuiz2_RegisterResult Register(string username, string email, string phone, string password)
        {
            return _db.QuerySingle<FTQuiz2_RegisterResult>("usp_User_Register",
                FTQuiz2_Db.P("@Username", username),
                FTQuiz2_Db.P("@Email", email),
                FTQuiz2_Db.P("@Phone", phone),
                FTQuiz2_Db.P("@Password", password));
        }

        // 0 = OK, 1 = invalid credentials, 2 = inactive
        public FTQuiz2_LoginResult Login(string identifier, string password)
        {
            return _db.QuerySingle<FTQuiz2_LoginResult>("usp_User_Login",
                FTQuiz2_Db.P("@Identifier", identifier),
                FTQuiz2_Db.P("@Password", password));
        }

        // null when the user does not exist (source: Rows.Count == 0 -> null)
        public FTQuiz2_User GetById(int userId)
        {
            FTQuiz2_UserRow row = _db.QueryFirstOrDefault<FTQuiz2_UserRow>("usp_User_GetById",
                FTQuiz2_Db.P("@UserId", userId));
            return row == null ? null : row.ToUser();
        }

        // 0 = OK, 1 = email taken, 2 = not found, 3 = invalid
        public int UpdateAccount(int userId, string email, string phone)
        {
            return _db.QuerySingle<FTQuiz2_CodeResult>("usp_User_UpdateAccount",
                FTQuiz2_Db.P("@UserId", userId),
                FTQuiz2_Db.P("@Email", email),
                FTQuiz2_Db.P("@Phone", phone)).ResultCode;
        }

        // 0 = OK, 1 = wrong current, 2 = not found, 3 = weak
        public int ChangePassword(int userId, string currentPassword, string newPassword)
        {
            return _db.QuerySingle<FTQuiz2_CodeResult>("usp_User_ChangePassword",
                FTQuiz2_Db.P("@UserId", userId),
                FTQuiz2_Db.P("@CurrentPassword", currentPassword),
                FTQuiz2_Db.P("@NewPassword", newPassword)).ResultCode;
        }

        // Newest first (ordering is done inside the procedure). Null/blank search or status = no filter.
        public List<FTQuiz2_User> GetUsers(string search, string status)
        {
            return _db.Query<FTQuiz2_UserRow>("usp_Admin_GetUsers",
                FTQuiz2_Db.P("@Search", search),
                FTQuiz2_Db.P("@Status", status)).Select(r => r.ToUser()).ToList();
        }

        // 0 = OK, 1 = not found, 2 = cannot act on self, 3 = cannot act on admin,
        // 4 = invalid status, 5 = not an admin
        public int SetUserStatus(int actingAdminId, int userId, string status)
        {
            return _db.QuerySingle<FTQuiz2_CodeResult>("usp_Admin_SetUserStatus",
                FTQuiz2_Db.P("@ActingAdminId", actingAdminId),
                FTQuiz2_Db.P("@UserId", userId),
                FTQuiz2_Db.P("@Status", status)).ResultCode;
        }

        // 0 = OK, 1 = not found, 2 = cannot act on self, 3 = cannot act on admin, 5 = not an admin
        public int DeleteUser(int actingAdminId, int userId)
        {
            return _db.QuerySingle<FTQuiz2_CodeResult>("usp_Admin_DeleteUser",
                FTQuiz2_Db.P("@ActingAdminId", actingAdminId),
                FTQuiz2_Db.P("@UserId", userId)).ResultCode;
        }
    }
}
