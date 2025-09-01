using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Sammlerplattform.Data;
using Sammlerplattform.Models.UserSettings;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class UserSettingsController(IWebHostEnvironment hostEnvironment, UserManager<UsingIdentityUser> userManager,
        IUserStore<UsingIdentityUser> userStore, SignInManager<UsingIdentityUser> signInManager, DbIdentityContext dbIdentityContext,
        ILogger<AccountController> logger, IEmailSender emailSender) : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;
        private readonly IUserStore<UsingIdentityUser> _userStore = userStore;
        private readonly SignInManager<UsingIdentityUser> _signInManager = signInManager;
        private readonly DbIdentityContext _dbIdentityContext = dbIdentityContext;
        private readonly ILogger<AccountController> _logger = logger;

        public IActionResult Profile(string statusMessage, string subscription)
        {
            UserWithPhoto? user = (from u in _userManager.Users
                                   join photo in _dbIdentityContext.UserPicture
                                   on u.Id equals photo.UsingIdentityUsers_ID into gj
                                   from subphoto in gj.DefaultIfEmpty()
                                   where u.Id == _userManager.GetUserId(User)
                                   select new UserWithPhoto { UsingIdentityUsers = u, UserPictured = subphoto.Photo }).FirstOrDefault();
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer mit Id zu laden '{_userManager.GetUserId(User)}'.");
            }

            List<IdentityUserClaim<string>> subItems = [.. from c in _dbIdentityContext.UserClaims
                                                        where c.UserId == user.UsingIdentityUsers.Id
                                                        select c];
            List<string> subscribtionIds = [];
            double currentFee = 0;
            double SubscribedDiskspaceFee = 0;

            foreach (IdentityUserClaim<string>? item in subItems)
            {
                //if (item.ClaimType != null && item.ClaimValue != null)
                //{
                //    string subscriptionId = PaymentService.GetSubFromSubItem(item.ClaimValue, _logger);
                //    currentFee += PaymentService.GetCurrentInvoice(subscriptionId, item.ClaimValue, _logger);

                //    // Cause Databasespace is counted at end of the month
                //    if (item.ClaimType.Equals("SubscribedDiskspace"))
                //    {
                //        int countUnits = (from e in _dbIdentityContext.PostcardEntity
                //                          where e.UsingIdentityUsers_ID == user.UsingIdentityUsers.Id
                //                          select e).Count();
                //        if (countUnits > 0)
                //        {
                //            countUnits /= 500;
                //            SubscribedDiskspaceFee = countUnits + 5;
                //        }
                //    }
                //}
            }
            currentFee /= 100;


            string subName = string.Empty;
            subName = subscription switch
            {
                "SubscribedDiskspace" => "Speicherplatz",
                "SubscribedAnalysisTool" => "Analysetool",
                _ => subscription,
            };
            ViewData["StatusMessage"] = statusMessage == "Cancel"
                ? "Kauf abgebrochen."
                : statusMessage == "CancelUpdate"
                    ? "Das Abonnement " + subName + " wird zum Ende des Abrechnungszeitraums gekündigt."
                    : statusMessage == "CancelCancelUpdate" ? "Das Abonnement " + subName + "  läuft weiter." : (object)statusMessage;

            ViewData["currentFee"] = currentFee + SubscribedDiskspaceFee;

            return View(user);
        }

        public async Task<IActionResult> ProfileChange(UserWithPhoto userWithPhoto)
        {
            string statusMessage = string.Empty;
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer mit Id zu laden '{_userManager.GetUserId(User)}'.");
            }

            if (userWithPhoto.UsingIdentityUsers.Email == null)
            {
                return NotFound($"Keine E-Mailadresse vorhanden");
            }

            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (userWithPhoto.UsingIdentityUsers.UserName != user.UserName)
            {
                IdentityResult usernameResult = new();
                if (string.IsNullOrEmpty(user.UserName))
                {
                    usernameResult = await _userManager.SetUserNameAsync(user, userWithPhoto.UsingIdentityUsers.UserName);
                }
                else
                {
                    user.UserName = userWithPhoto.UsingIdentityUsers.UserName;
                    usernameResult = await _userManager.UpdateAsync(user);
                }

                if (!usernameResult.Succeeded)
                {
                    statusMessage = "Unerwarteter Fehler, beim Ändern des Benuternamens";
                    return RedirectToAction(nameof(Profile), new { statusMessage });
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            statusMessage = "Ihr Profil wurde aktualisiert";

            return RedirectToAction(nameof(Profile), new { statusMessage });
        }


        public async Task<IActionResult> DownloadPersonalData()
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Only include personal data for download
            Dictionary<string, string> personalData = [];
            IEnumerable<System.Reflection.PropertyInfo> personalDataProps = typeof(UsingIdentityUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (System.Reflection.PropertyInfo? p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
            foreach (UserLoginInfo l in logins)
            {
                personalData.Add($"{l.LoginProvider} externer Login Provider Key", l.ProviderKey);
            }

            //Für 2-Faktor Authentifizierung
            string? authKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (authKey != null)
            {
                personalData.Add($"Authenticator Key", authKey);
            }

            Response.Headers.Append("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }

        public async Task<IActionResult> DeletePersonalData()
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            ViewData["RequirePassword"] = await _userManager.HasPasswordAsync(user);
            ViewData["userId"] = user.Id;
            return View();
        }
        public async Task<IActionResult> DeletePersonalDataSubmit(string Password)
        {
            string statusMessage = "";

            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            bool RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Password))
                {
                    ModelState.AddModelError(string.Empty, "Falsches Passwort.");
                    return RedirectToAction(nameof(Profile));
                }
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                UserPicture? userPictureJoin = (from u in _dbIdentityContext.Users
                                                join p in _dbIdentityContext.UserPicture
                                                on u.Id equals p.UsingIdentityUsers_ID
                                                where u.Id == user.Id
                                                select p).FirstOrDefault();
                if (userPictureJoin != null)
                {
                    _ = _dbIdentityContext.Remove(userPictureJoin);
                    _ = await _dbIdentityContext.SaveChangesAsync();
                }

                IdentityResult result = await _userManager.DeleteAsync(user);
                string userId = await _userManager.GetUserIdAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unerwarteter Fehler trat beim Löschen des Profils auf.");
                }

                await _signInManager.SignOutAsync();
                _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);
                statusMessage = "Löschung erfolgreich.";
                if (user.Email != null)
                {
                    await emailSender.SendEmailAsync(
                        user.Email,
                        "Account wurde gelöscht",
                        "Ihr Account mit allen Abos wurde gelöscht.");
                }
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError("Abgebrochen mit Exception {ex}", ex);
                statusMessage = "Löschung wurde abgebrochen. Fehler wurde gemeldet.";
            }

            return RedirectToAction("Frontpage", "Home", new { statusMessage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhotoUploading(string id, IFormFile formFile)
        {
            byte[] photo;
            if (formFile != null)
            {
                using (MemoryStream ms = new())
                {
                    formFile.CopyTo(ms);
                    ms.Position = 0;
                    using MagickImage image = new(ms);
                    image.Quality = 30;
                    if (image.Width > image.Height)
                    {
                        image.Scale(498, 322);
                    }
                    else if (image.Height > image.Width)
                    {
                        image.Scale(498, 708);
                        image.Crop(498, 322);
                    }
                    image.Write(ms);
                    photo = ms.ToArray();
                }

                UserPicture? photoSelect = (from p in _dbIdentityContext.UserPicture
                                            where p.UsingIdentityUsers_ID == id
                                            select p).FirstOrDefault();

                if (photoSelect is null)
                {
                    UserPicture userPicture = new()
                    {
                        UsingIdentityUsers_ID = id,
                        Photo = photo
                    };
                    _ = _dbIdentityContext.Add(userPicture);
                }
                else
                {
                    photoSelect.Photo = photo;
                }

                _ = await _dbIdentityContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Profile));
        }

        public async Task<IActionResult> ChangePassword(string statusMessage)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer zu finden mit ID '{_userManager.GetUserId(User)}'.");
            }

            ViewData["StatusMessage"] = statusMessage;

            return View();
        }

        public async Task<IActionResult> ChangePasswordSubmit(ChangePasswordModel changePasswordModel)
        {
            string statusMessage = string.Empty;

            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
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

            return RedirectToAction(nameof(ChangePassword), new { statusMessage = "Passwort erfolgreich geändert." });
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
                    "ConfirmMailChange", "Account",
                    new { userId, email = changeEMailModel.NewEmail, code },
                    protocol: Request.Scheme);
                if (callbackUrl != null)
                {
                    await emailSender.SendEmailAsync(
                    changeEMailModel.NewEmail,
                    "Bestätige deine E-Mail",
                    $"Bitte bestätige deinen Account durch <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> hier klicken</a>.");
                }

                return RedirectToAction(nameof(ChangeEMail), new { statusMessage = "Bestätungs-E-Mail wurde versandt. Bitte prüfe deinen Briefkasten." });
            }

            return RedirectToAction(nameof(ChangeEMail), new { statusMessage = "Deine E-Mail wurde nicht geändert." });
        }
    }
}