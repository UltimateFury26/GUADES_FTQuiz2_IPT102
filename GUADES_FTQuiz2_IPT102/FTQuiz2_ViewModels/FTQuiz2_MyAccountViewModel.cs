using System.ComponentModel.DataAnnotations;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// MyAccount.aspx fields. Two forms share one page, so this model carries both; the
    /// controller removes the other form's keys from ModelState before checking IsValid
    /// (the MVC stand-in for ValidationGroup="AccountInfo" / "ChangePassword").
    /// Expressions and messages are copied from the source validators.
    /// </summary>
    public class FTQuiz2_MyAccountViewModel
    {
        // ---- read-only (never posted; reloaded from the database on every redisplay) ----
        public string Username { get; set; }

        // ---- Account Information form ----
        [Required(ErrorMessage = "Enter your email.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Enter a valid email, like name@example.com.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter your phone number.")]
        [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "Use 11 digits starting with 09.")]
        public string Phone { get; set; }

        // ---- Change Password form ----
        [Required(ErrorMessage = "Enter your current password.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Choose a new password.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*[0-9]).{8,100}$",
            ErrorMessage = "That password doesn't meet the requirements.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Type the new password again.")]
        [Compare("NewPassword", ErrorMessage = "The passwords don't match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
