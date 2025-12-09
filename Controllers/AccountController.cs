using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Resources;
using Sammlerplattform.Models.Account;
using Sammlerplattform.Models.UserSettings;
using System.Text;
using System.Text.Encodings.Web;
using Sammlerplattform.Models;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<UsingIdentityUser> _signInManager;
        private readonly UserManager<UsingIdentityUser> _userManager;
        private readonly IUserStore<UsingIdentityUser> _userStore;
        private readonly IUserEmailStore<UsingIdentityUser> _emailStore;
        private readonly IEmailSender _emailSender; 
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly IHtmlLocalizer<SharedResources> _localizerHtml;

        public AccountController(
            UserManager<UsingIdentityUser> userManager,
            IUserStore<UsingIdentityUser> userStore,
            SignInManager<UsingIdentityUser> signInManager,
            IEmailSender emailSender,
            IStringLocalizer<SharedResources> localizer,
            IHtmlLocalizer<SharedResources> htmlLocalizer)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _emailSender = emailSender;
            _localizer = localizer;
            _localizerHtml = htmlLocalizer;
        }

        public async Task<ViewResult> Login(Status status)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            HandleStatus(status);

            return View();
        }

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Login));
            }
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return RedirectToAction(nameof(Login), new Status() { Message = "Error_EmailPassword_Necessary" } );
            }

            UsingIdentityUser? existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return RedirectToAction(nameof(Login), new { statusMessage = "Error_User_Exists" });
            }

            UsingIdentityUser user = CreateUser();

            await _userStore.SetUserNameAsync(user, model.UserName, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                string statusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login), new { statusMessage });
            }

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                bool success = await SendConfirmationEmailAsync(user);
                string message = success
                    ? _localizer["Success_ConfirmationEmail_Sent"]
                    : _localizer["Error_ConfirmationEmail_NotSend"];

                return RedirectToAction(nameof(Login), new { statusMessage = message });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect("/");
        }

        public async Task<IActionResult> LoginPost(LoginViewModel model)
        {
            //für Test
            model.Password = "123456";
            model.Email = "tester@web.de";


            if (model.Password == null || model.Email == null)
            {
                return RedirectToAction(nameof(Login), new { statusMessage = "Error_Password_Empty" });
            }

            UsingIdentityUser? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return RedirectToAction(nameof(Login));
            }

            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                //return RedirectToAction("Profile", "UserSettings");
                return RedirectToAction("Index", "CollectionAreaDatabase");
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Logout));
            }
            else
            {
                bool emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                if (emailConfirmed is false)
                {
                    return RedirectToAction(nameof(Login), new { errorMessage = "Error_Email_NotConfirmed" });
                }
                else
                {
                    return RedirectToAction(nameof(Login), new { statusMessage = "Success_Email_Confirmed", statusCode = 200 });
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Frontpage", "Home", new { Message = "Success_Logout", Code = 200 });
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            UsingIdentityUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User not found.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);
            string statusMessage = result.Succeeded ? "Success_Email_Confirmed" : "Error_Email_NotConfirmed";

            return RedirectToAction(nameof(Login), new { statusMessage });
        }

        public async Task<IActionResult> ResendEmailSubmit(LoginViewModel model)
        {
            if (model.Email is null)
            {
                return RedirectToAction(nameof(Login), new { statusMessage = "Error_Email_Missing" });
            }

            UsingIdentityUser? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(Login), new { statusMessage = "Error_Email_NotFound" });
            }

            _ = await SendConfirmationEmailAsync(user);

            return RedirectToAction(nameof(Login), new { statusMessage = "Success_ConfirmationEmail_Sent", statusCode = 200 });
        }

        public async Task<IActionResult> ForgotPasswordSubmit(LoginViewModel loginViewModel)
        {
            if (loginViewModel.Email is null)
            {
                return RedirectToAction(nameof(Login));
            }

            UsingIdentityUser? user = await _userManager.FindByEmailAsync(loginViewModel.Email);
            if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return RedirectToAction(nameof(Login));
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string? callbackUrl = Url.Action(
                        nameof(ResetPassword),
                        "Account",
                        new { code },
                        protocol: HttpContext.Request.Scheme
                        );
            if (callbackUrl != null)
            {
                string htmlBody = _localizerHtml["PasswordResetHtml", HtmlEncoder.Default.Encode(callbackUrl)].Value;
                await _emailSender.SendEmailAsync(
                    loginViewModel.Email,
                    _localizer["Reset your password."],
                    htmlBody);
            }

            return RedirectToAction(nameof(Login));
        }

        public IActionResult ResetPassword(string? code)
        {
            if (code == null)
            {
                return RedirectToAction(nameof(Login), new { statusMessage = "Error_Code_Missing" });
            }
            else
            {
                ResetPasswordModel resetPasswordModel = new()
                {
                    Email = string.Empty,
                    Password = string.Empty,
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return View(resetPasswordModel);
            }
        }

        public async Task<IActionResult> ResetPasswordSubmit(ResetPasswordModel resetPasswordModel)
        {
            UsingIdentityUser? user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult result = await _userManager.ResetPasswordAsync(user, code, resetPasswordModel.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Login));
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction(nameof(ResetPassword), new { code });
        }

        public async Task<IActionResult> ConfirmMailChange(string userId, string email, string code)
        {
            UsingIdentityUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login), new { statusMessage = "Error_User_NotFound" });
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                return RedirectToAction("ChangeEmail", "UserSettings", new { statusMessage = "Error_Email_Change" });
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("ChangeEmail", "UserSettings", new { statusMessage = "Success_Email_Change", statusCode = 200 });
        }
        private IUserEmailStore<UsingIdentityUser> GetEmailStore()
        {
            return !_userManager.SupportsUserEmail
                ? throw new NotSupportedException("The standard UI requires a user account with email support.")
                : (IUserEmailStore<UsingIdentityUser>)_userStore;
        }

        private UsingIdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<UsingIdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Kann keine Instanz von '{nameof(UsingIdentityUser)}' kreieren. Stelle sicher, dass '{nameof(UsingIdentityUser)}' keine abstrakte Klasse ist und einen parameterlosen Konsturktor aufweist oder alternativ überschreibe die Registrierungsseite in AccountController/Register.cshtml");
            }
        }

        private async Task<bool> SendConfirmationEmailAsync(UsingIdentityUser user)
        {
            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string? callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId, code = encodedCode },
                protocol: HttpContext.Request.Scheme
            );

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                return false;
            }

            string htmlBody = _localizerHtml["ConfirmAccountHtml", HtmlEncoder.Default.Encode(callbackUrl)].Value;
            await _emailSender.SendEmailAsync(
                user.Email!,
                _localizer["Message_ConfirmEmail"],
                htmlBody
            );

            return true;
        }

        private void HandleStatus(Status status)
        {
            if (!string.IsNullOrEmpty(status.Message))
            {
                ViewData["StatusMessage"] = _localizerHtml[status.Message];
                ViewData["StatusCode"] = status.Code;
            }
        }
    }
}

