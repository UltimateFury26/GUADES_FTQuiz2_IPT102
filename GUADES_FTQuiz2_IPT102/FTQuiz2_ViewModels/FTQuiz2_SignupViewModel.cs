using System.ComponentModel.DataAnnotations;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// Signup.aspx fields. Expressions and messages are copied from the source validators
    /// (they mirror fn_IsValidUsername / fn_IsValidEmail / fn_IsValidPhone / fn_IsStrongPassword);
    /// usp_User_Register re-checks every rule.
    /// </summary>
    public class FTQuiz2_SignupViewModel
    {
        [Required(ErrorMessage = "Choose a username.")]
        [RegularExpression(@"^[A-Za-z0-9_]{4,50}$",
            ErrorMessage = "Use 4 to 50 characters: letters, numbers or underscores only.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Enter your email.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Enter a valid email, like name@example.com.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter your phone number.")]
        [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "Use 11 digits starting with 09.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Choose a password.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*[0-9]).{8,100}$",
            ErrorMessage = "That password doesn't meet the requirements.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Type your password again.")]
        [Compare("Password", ErrorMessage = "The passwords don't match.")]
        public string ConfirmPassword { get; set; }
    }
}
