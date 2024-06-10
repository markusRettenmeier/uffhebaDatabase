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

        public async Task<ViewResult> Login(string ErrorMessage, string statusMessage, string? returnUrl = null)
        {

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            _ = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["StatusMessage"] = statusMessage;

            return View();
        }

        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            string errors = string.Empty;

            if (ModelState.IsValid)
            {
                if (model.Email != null)
                {
                    UsingIdentityUser? checkUser = await _userManager.FindByEmailAsync(model.Email);
                    if (checkUser != null)
                    {
                        return RedirectToAction(nameof(Login));
                    }
                }

                UsingIdentityUser user = CreateUser();

                await _userStore.SetUserNameAsync(user, model.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
                if (model.Password != null)
                {
                    IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        if (model.Email != null)
                        {
                            if (_userManager.Options.SignIn.RequireConfirmedAccount)
                            {
                                bool sentConfirmationMailSuccessfully = await RegisterConfirmation(model.Email);
                                return sentConfirmationMailSuccessfully == true
                                    ? RedirectToAction(nameof(Login), new { statusMessage = "Bestätigungs-E-Mail wurde versandt." })
                                    : (IActionResult)RedirectToAction(nameof(Login), new { ErrorMessage = errors, statusMessage = "Es ist ein Fehler aufgetreten." });
                            }
                            else
                            {
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return LocalRedirect(returnUrl);
                            }
                        }
                    }
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        errors += error.Description;
                    }
                }
            }

            return RedirectToAction(nameof(Login), new { ErrorMessage = errors });
        }

        public async Task<bool> RegisterConfirmation(string email)
        {
            if (email == null)
            {
                return false;
            }

            UsingIdentityUser? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string? callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId, code },
                protocol: HttpContext.Request.Scheme
                );
            callbackUrl ??= string.Empty;
            await _emailSender.SendEmailAsync(email, "Bestätige deine E-Mail", $"Bitte bestätige deinen Account durch <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>hier klicken</a>.");
            return true;
        }


        public async Task<IActionResult> LoginPost(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                if (model.Email != null && model.Password != null)
                {
                    Task<UsingIdentityUser?> userName = _userManager.FindByEmailAsync(model.Email);
                    if (userName.Result != null && userName.Result.UserName != null)
                    {
                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(userName.Result.UserName, model.Password, false, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User logged in.");
                            //return LocalRedirect(returnUrl);
                            return RedirectToAction("Profile", "UserSettings");
                        }
                        if (result.IsLockedOut)
                        {
                            _logger.LogWarning("User account locked out.");
                            return RedirectToAction(nameof(Logout), new { StatusMessage = "Sie wurde erfolgreich ausgeloggt-" });
                        }
                        else
                        {
                            UsingIdentityUser? user = await _userManager.FindByEmailAsync(model.Email);
                            if (user is not null)
                            {
                                bool emailNotConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                                if (emailNotConfirmed is false)
                                {
                                    ModelState.AddModelError(string.Empty, "E-Mail wurde nicht bestätigt.");
                                    return RedirectToAction(nameof(Login), new { ErrorMessage = "E-Mail wurde nicht bestätigt." });
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Das Passwort ist falsch");
                                    return RedirectToAction(nameof(Login), new { statusMessage = "Account nicht gefunden oder Passwort inkorrekt." });
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalider Login-Versuch.");
                                return RedirectToAction(nameof(Login), new { ErrorMessage = "Invalider Login-Versuch." });
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction(nameof(Login), new { statusMessage = "Account nicht gefunden oder Passwort inkorrekt." });
                    }

                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction(nameof(Login), new { ErrorMessage = "Es wurde keine E-Mail angegeben oder es liegt ein schwerer Fehler vor" });
        }

        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToAction("Frontpage", "Home", new { statusMessage = "Erfolgreich ausgeloggt." });
            }
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
                    $"überschreibe die Registrierungsseite in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<UsingIdentityUser> GetEmailStore()
        {
            return !_userManager.SupportsUserEmail
                ? throw new NotSupportedException("Das Stadard-UI benötigt einen Benutzerplatz mit E-Mail-Unterstützung.")
                : (IUserEmailStore<UsingIdentityUser>)_userStore;
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(Login));
            }

            UsingIdentityUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);
            string statusMessage = result.Succeeded ? "Vielen Dank, für die E-Mailbestätigung." : "Fehler bezüglich Ihrer E-Mail.";

            return RedirectToAction(nameof(Login), new { statusMessage });
        }

        public async Task<IActionResult> ResendEmailSubmit(LoginViewModel model)
        {
            if (model.Email != null)
            {
                UsingIdentityUser? user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Bestätigungs-E-Mail versendet. Bitte prüfe deine E-Mails.");
                    return RedirectToAction("Login", "Accounts", new { ErrorMessage = "Angegebene E-Mail nicht gefunden" });
                }


                string userId = await _userManager.GetUserIdAsync(user);
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string? callbackUrl = Url.Action(
                            nameof(ConfirmEmail),
                            "Account",
                            new { userId, code },
                            protocol: HttpContext.Request.Scheme
                            );
                if (callbackUrl != null)
                {
                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Bestätige deine E-Mail",
                        $"Bitte bestätige deinen Account durch <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>hier klicken</a>.");
                }

                ModelState.AddModelError(string.Empty, "Bestätigungs-E-Mail versendet. Bitte prüfe deine E-Mails.");
            }

            return RedirectToAction(nameof(Login), new { StatusMessage = "Bestätigungs-E-Mail wurde versandt." });
        }

        public async Task<IActionResult> ForgotPasswordSubmit(LoginViewModel loginViewModel)
        {
            if (loginViewModel.Email != null)
            {
                UsingIdentityUser? user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(Login), new { ErrorMessage = "Bitte prüfe deine E-Mail, um dein Passwort zurückzusetzen." });
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
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ResetPassword));
            }

            UsingIdentityUser? user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
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

        public async Task<IActionResult> ChangeEMail(string statusMessage)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            string email = await _userManager.GetEmailAsync(user) ?? throw new NullReferenceException("email in LoadAsync");
            ChangeEMailModel changeEMailModel = new()
            {
                Email = email,
                IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user)
            };

            ViewData["StatusMessage"] = statusMessage;
            ViewData["RequirePassword"] = await _userManager.HasPasswordAsync(user);
            return View(changeEMailModel);
        }

        public async Task<IActionResult> ChangeEMailSubmit(ChangeEMailModel changeEMailModel)
        {
            string statusMessage = string.Empty;
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            bool RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword && changeEMailModel.Password != null)
            {
                if (!await _userManager.CheckPasswordAsync(user, changeEMailModel.Password))
                {
                    ModelState.AddModelError(string.Empty, "Falsches Passwort.");
                    return RedirectToAction(nameof(Login));
                }
            }

            string? email = await _userManager.GetEmailAsync(user);
            if (changeEMailModel.NewEmail != email)
            {
                string userId = await _userManager.GetUserIdAsync(user);
                string code = await _userManager.GenerateChangeEmailTokenAsync(user, changeEMailModel.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string? callbackUrl = Url.Action(
                    nameof(ConfirmMailChange), "Account",
                    new { userId, email = changeEMailModel.NewEmail, code },
                    protocol: Request.Scheme);
                if (callbackUrl != null)
                {
                    await _emailSender.SendEmailAsync(
                    changeEMailModel.NewEmail,
                    "Bestätige deine E-Mail",
                    $"Bitte bestätige deinen Account durch <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> hier klicken</a>.");
                }

                statusMessage = "Bestätungs-E-Mail wurde versandt. Bitte prüfe deinen Briefkasten.";
                return RedirectToAction(nameof(ChangeEMail), new { statusMessage });
            }

            statusMessage = "Deine E-Mail wurde nicht geändert.";
            return RedirectToAction(nameof(ChangeEMail), new { statusMessage });
        }

        public async Task<IActionResult> ChangePassword(string statusMessage)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction("./SetPassword");
            }

            ViewData["StatusMessage"] = statusMessage;

            return View();
        }

        public async Task<IActionResult> ChangePasswordSubmit(ChangePasswordModel changePasswordModel)
        {
            string statusMessage = string.Empty;
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ChangePassword), new { errors = "Passt nicht" });
            }

            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            if (changePasswordModel.OldPassword != null && changePasswordModel.NewPassword != null)
            {
                IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordModel.OldPassword, changePasswordModel.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (IdentityError error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        statusMessage += error.Description;
                        _logger.LogError("ChangePassword nicht erfolgreich mit Fehler {error.Description}", error.Description);
                    }
                    return RedirectToAction(nameof(ChangePassword), new { statusMessage });
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");

            return RedirectToAction(nameof(ChangePassword), new { statusMessage = "Passwort erfolgreich geändert." });
        }

        public async Task<IActionResult> ConfirmMailChange(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                return RedirectToAction("Frontpage", "Collection");
            }

            UsingIdentityUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                ViewData["StatusMessage"] = "Fehler beim Ändern der E-Mail";
                return RedirectToAction(nameof(ChangeEMail));
            }

            await _signInManager.RefreshSignInAsync(user);
            ViewData["StatusMessage"] = "Thank you for confirming your email change.";
            return RedirectToAction(nameof(ChangeEMail));
        }
    }
}

