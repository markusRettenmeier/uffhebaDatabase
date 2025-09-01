using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Sammlerplattform.Models.Account;
using Sammlerplattform.Models.UserSettings;
using System.Text;
using System.Text.Encodings.Web;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<UsingIdentityUser> _signInManager;
        private readonly UserManager<UsingIdentityUser> _userManager;
        private readonly IUserStore<UsingIdentityUser> _userStore;
        private readonly IUserEmailStore<UsingIdentityUser> _emailStore;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<UsingIdentityUser> userManager,
            IUserStore<UsingIdentityUser> userStore,
            SignInManager<UsingIdentityUser> signInManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }
        private IUserEmailStore<UsingIdentityUser> GetEmailStore()
        {
            return !_userManager.SupportsUserEmail
                ? throw new NotSupportedException("Das Stadard-UI benötigt einen Benutzerplatz mit E-Mail-Unterstützung.")
                : (IUserEmailStore<UsingIdentityUser>)_userStore;
        }

        public async Task<ViewResult> Login(string errorMessage, string statusMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["StatusMessage"] = statusMessage;

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
                return RedirectToAction(nameof(Login), new { ErrorMessage = "E-Mail und Passwort sind erforderlich." });
            }

            UsingIdentityUser? existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return RedirectToAction(nameof(Login), new { ErrorMessage = "Ein Benutzer mit dieser E-Mail existiert bereits." });
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

                string errorMessages = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login), new { errorMessages });
            }

            _logger.LogInformation("User created a new account with password.");

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                bool success = await SendConfirmationEmailAsync(user);
                string message = success
                    ? "Bestätigungs-E-Mail wurde versandt."
                    : "Es ist ein Fehler beim Versenden der Bestätigungs-E-Mail aufgetreten.";

                return RedirectToAction(nameof(Login), new { statusMessage = message });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect("/");
        }

        private UsingIdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<UsingIdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Kann keine Instanz von '{nameof(UsingIdentityUser)}' kreieren. " +
                    $"Stelle sicher, dass '{nameof(UsingIdentityUser)}' keine abstrakte Klasse ist und einen parameterlosen Konsturktor aufweist oder alternativ " +
                    $"überschreibe die Registrierungsseite in AccountController/Register.cshtml");
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

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Bestätige deine E-Mail",
                $"Bitte bestätige deinen Account durch <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>hier klicken</a>."
            );

            return true;
        }

        public async Task<IActionResult> LoginPost(LoginViewModel model)
        {
            if (model.Password == null || model.Email == null)
            {
                return RedirectToAction(nameof(Login), new { errorMessage = "Passwort leer." });
            }

            UsingIdentityUser? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return RedirectToAction(nameof(Login));
            }

            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile", "UserSettings");
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
                    ModelState.AddModelError(string.Empty, "E-Mail wurde nicht bestätigt.");
                    return RedirectToAction(nameof(Login), new { errorMessage = "E-Mail wurde nicht bestätigt." });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Das Passwort ist falsch");
                    return RedirectToAction(nameof(Login), new { statusMessage = "Passwort inkorrekt." });
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Frontpage", "Home", new { statusMessage = "Erfolgreich ausgeloggt." });
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            UsingIdentityUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Nutzer nicht gefunden.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);
            string statusMessage = result.Succeeded ? "Vielen Dank, für die E-Mailbestätigung." : "Fehler bezüglich Ihrer E-Mail.";

            return RedirectToAction(nameof(Login), new { statusMessage });
        }

        public async Task<IActionResult> ResendEmailSubmit(LoginViewModel model)
        {
            if (model.Email is null)
            {
                return RedirectToAction(nameof(Login), new { errorMessage = "Email fehlt." });
            }

            UsingIdentityUser? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(Login), new { errorMessage = "Angegebene E-Mail nicht gefunden" });
            }

            _ = await SendConfirmationEmailAsync(user);

            return RedirectToAction(nameof(Login), new { statusMessage = "Bestätigungs-E-Mail wurde versandt." });
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
                await _emailSender.SendEmailAsync(
                    loginViewModel.Email,
                    "Setze dein Passwort zurück",
                    $"Bitte setze dein Passwort zurück durch by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>hier klicken</a>.");
            }

            return RedirectToAction(nameof(Login));
        }

        public IActionResult ResetPassword(string? code)
        {
            if (code == null)
            {
                return BadRequest("Ein Code muss beim Zurücksetzen mitgegeben werden");
            }
            else
            {
                ResetPasswordModel resetPasswordModel = new()
                {
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
                return NotFound($"Fehler: Nutzer mit ID '{userId}' fehlt.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                return RedirectToAction("ChangeEmail", "UserSettings", new { statusMessage = "Fehler bei Änderung der E-Mail" });
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("ChangeEmail", "UserSettings", new { statusMessage = "Änderung der E-Mail nicht erfolgreich." });
        }

    }
}

