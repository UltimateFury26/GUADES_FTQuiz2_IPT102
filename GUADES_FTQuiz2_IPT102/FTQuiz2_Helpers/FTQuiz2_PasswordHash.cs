using System;
using System.Security.Cryptography;
using System.Text;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers
{
    /// <summary>
    /// Byte-for-byte mirror of dbo.fn_HashPassword (ADR-002):
    ///   HASHBYTES('SHA2_256', @Password + CONVERT(NVARCHAR(36), @Salt))
    /// For tests / future migration ONLY. Registration, login and password change
    /// stay inside the stored procedures; nothing in the app writes a hash from C#.
    /// SQL concatenates NVARCHAR, so the hashed bytes are UTF-16LE, and
    /// CONVERT(NVARCHAR(36), uniqueidentifier) is UPPERCASE hyphenated text.
    /// </summary>
    public static class FTQuiz2_PasswordHash
    {
        public static byte[] Compute(string password, Guid salt)
        {
            string material = (password ?? string.Empty) + salt.ToString().ToUpperInvariant();
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.Unicode.GetBytes(material));
            }
        }

        public static bool Verify(string password, Guid salt, byte[] expected)
        {
            if (expected == null || expected.Length != 32) { return false; }

            byte[] actual = Compute(password, salt);
            int diff = 0;
            for (int i = 0; i < 32; i++) { diff |= actual[i] ^ expected[i]; }
            return diff == 0;
        }
    }
}
