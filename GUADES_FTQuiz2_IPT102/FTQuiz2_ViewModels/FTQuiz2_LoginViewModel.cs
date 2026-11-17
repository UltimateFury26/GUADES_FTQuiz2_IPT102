using System.ComponentModel.DataAnnotations;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// Login.aspx fields. Required messages are the source RequiredFieldValidator texts.
    /// The source has no "remember me" option, so none is offered here.
    /// </summary>
    public class FTQuiz2_LoginViewModel
    {
        [Required(ErrorMessage = "Enter your username or email.")]
        public string Identifier { get; set; }

        [Required(ErrorMessage = "Enter your password.")]
        public string Password { get; set; }
    }
}
