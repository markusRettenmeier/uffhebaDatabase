using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using Stripe;
using System.Linq.Dynamic.Core;
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

        public async Task<IActionResult> Profile(string statusMessage, string subscription)
        {
            UserWithPhoto user = await (from u in _userManager.Users
                                        join photo in _dbIdentityContext.UserPicture
                                        on u.Id equals photo.UsingIdentityUsers_ID into gj
                                        from subphoto in gj.DefaultIfEmpty()
                                        where u.Id == _userManager.GetUserId(User)
                                        select new UserWithPhoto { UsingIdentityUsers = u, UserPictured = subphoto.Photo }).FirstAsync();
            if (user == null)
            {
                return NotFound($"Unmöglich, Nutzer mit Id zu laden '{_userManager.GetUserId(User)}'.");
            }

            List<IdentityUserClaim<string>> subItems = [.. (from c in _dbIdentityContext.UserClaims
                                                        where c.UserId == user.UsingIdentityUsers.Id
                                                        select c)];
            List<string> subscribtionIds = [];
            double currentFee = 0;
            double SubscribedDiskspaceFee = 0;

            foreach (IdentityUserClaim<string>? item in subItems)
            {
                if (item.ClaimType != null && item.ClaimValue != null)
                {
                    string subscriptionId = PaymentService.GetSubFromSubItem(item.ClaimValue, _logger);
                    currentFee += PaymentService.GetCurrentInvoice(subscriptionId, item.ClaimValue, _logger);

                    // Cause Databasespace is counted at end of the month
                    if (item.ClaimType.Equals("SubscribedDiskspace"))
                    {
                        int countUnits = (from e in _dbIdentityContext.PostcardEntity
                                          where e.UsingIdentityUsers_ID == user.UsingIdentityUsers.Id
                                          select e).Count();
                        if (countUnits > 0)
                        {
                            countUnits /= 500;
                            SubscribedDiskspaceFee = countUnits + 5;
                        }
                    }
                }
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
                CustomerService service = new();
                if (user.StripeCustomer_ID != null)
                {
                    try
                    {
                        _ = service.Delete(user.StripeCustomer_ID);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Abgebrochen mit Exception {ex}", ex);
                    }
                }

                List<PostcardModel> CollectionUser = [.. (from u in _userManager.Users
                                                  join e in _dbIdentityContext.PostcardEntity on u.Id equals e.UsingIdentityUsers_ID
                                                  join p in _dbIdentityContext.PostcardPotential on e.PostcardPotential_ID equals p.PostcardPotential_ID
                                                  join i in _dbIdentityContext.PostcardImprint on p.PostcardImprint_ID equals i.Image_ID into leftOuterImprint
                                                    from subImprint in leftOuterImprint.DefaultIfEmpty()
                                                  where u.Id == user.Id
                                                  select new PostcardModel
                                                  {
                                                      PostcardEntity = e,
                                                      PostcardPotential = p,
                                                      PostcardImprint = subImprint,
                                                      UsingIdentityUser = u,
                                                      ProductPictureList = (from Scan in _dbIdentityContext.ProductPicture
                                                                          join pe in _dbIdentityContext.PostcardEntity
                                                                          on Scan.PostcardEntity_ID equals pe.PostcardEntity_ID
                                                                          where pe.PostcardEntity_ID == e.PostcardEntity_ID
                                                                          select Scan).ToList()
                                                  })];
                if (CollectionUser.Count > 0)
                {
                    foreach (PostcardModel? item in CollectionUser)
                    {
                        foreach (ProductPicture scan in item.ProductPictureList)
                        {
                            if (scan.Frontside)
                            {
                                System.IO.File.Delete("wwwroot/images/Klein/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                                System.IO.File.Delete("wwwroot/images/Thumbnail/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                                System.IO.File.Delete("wwwroot/images/Normal/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                            }
                            else
                            {
                                System.IO.File.Delete("wwwroot/images/Normal/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                            }
                            _ = _dbIdentityContext.Remove(scan);
                            _ = await _dbIdentityContext.SaveChangesAsync();
                        }
                        _ = _dbIdentityContext.Remove(item.PostcardPotential);
                        _ = await _dbIdentityContext.SaveChangesAsync();
                        if (item.PostcardImprint != null)
                        {
                            _ = _dbIdentityContext.Remove(item.PostcardImprint);
                            _ = await _dbIdentityContext.SaveChangesAsync();
                        }
                    }
                }

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
    }
}