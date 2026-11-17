using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;
using GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels;

namespace GUADES_FTQuiz2_IPT102.Controllers
{
    /// <summary>
    /// Port of Signup.aspx / Signup.aspx.cs (a GuestPage). Success does NOT sign the person in:
    /// like the source it goes to Login with the "Account created" notice.
    /// </summary>
    public class FTQuiz2_SignupController : FTQuiz2_GuestController
    {
        private const string InvalidInputMessage = "Something in the form isn't valid. Check each field and try again.";
        private const string DatabaseMessage = "We couldn't reach the database right now. Try again in a moment.";

        [HttpGet]
        public ActionResult Index()
        {
            ClearNotice();
            ViewBag.FTQuiz2_FocusId = "Username";
            return View(new FTQuiz2_SignupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FTQuiz2_SignupViewModel model)
        {
            ClearNotice();

            if (!ModelState.IsValid) { return View(model); }

            FTQuiz2_RegisterResult result;

            try
            {
                // Text fields are trimmed; the password is sent exactly as typed.
                result = new FTQuiz2_UserRepository().Register(
                    model.Username.Trim(),
                    model.Email.Trim(),
                    model.Phone.Trim(),
                    model.Password);
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                ShowError(DatabaseMessage);
                return View(model);
            }

            switch (result.ResultCode)
            {
                case FTQuiz2_Constants.RegisterCodes.Ok:
                    return RedirectToAction("Index", "FTQuiz2_Login",
                        new { notice = FTQuiz2_Constants.Notices.SignedUp });

                case FTQuiz2_Constants.RegisterCodes.UsernameTaken:
                    ModelState.AddModelError("Username", "That username is already taken. Try another.");
                    ViewBag.FTQuiz2_FocusId = "Username";
                    break;

                case FTQuiz2_Constants.RegisterCodes.EmailTaken:
                    ModelState.AddModelError("Email", "An account with that email already exists. Log in instead.");
                    ViewBag.FTQuiz2_FocusId = "Email";
                    break;

                default:
                    ShowError(InvalidInputMessage);
                    break;
            }

            return View(model);
        }

        private void ShowError(string message)
        {
            ViewBag.FTQuiz2_NoticeText = message;
            ViewBag.FTQuiz2_NoticeKind = "error";
        }

        // Signup shows no incoming notices (NoticeFilter may have filled one from the query string).
        private void ClearNotice()
        {
            ViewBag.FTQuiz2_NoticeText = null;
            ViewBag.FTQuiz2_NoticeKind = null;
        }

        private static bool IsDatabaseError(Exception ex)
        {
            for (Exception e = ex; e != null; e = e.InnerException)
            {
                if (e is SqlException) { return true; }
            }
            return false;
        }
    }
}
