using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PasskeyProcessees;
using System.Reflection;
using System.Text.Json;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class UserSettingsController(UserManager<UsingIdentityUser> userManager
        , SignInManager<UsingIdentityUser> signInManager
        , IEmailSender emailSender
        , IStringLocalizer<SharedResources> stringLocalizer
        , ITrackEventsCSV trackEvents
        , IProcessFidoCredential processFidoCredential
        , IProcessCollectionItemEntity processCollectionItemEntity) : Controller
    {
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;
        private readonly SignInManager<UsingIdentityUser> _signInManager = signInManager;

        [HandleStatus]
        public IActionResult Profile()
        {
            UsingIdentityUser? user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToAction("Login", "Passkey", new { statusmessage = "Error_User_NotFound" });
            }

            return View(user);
        }

        public async Task<IActionResult> ProfileChange(UsingIdentityUser usingIdentityUser)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Passkey", new { statusmessage = "Error_Place_NotFound" });
            }

            if (string.IsNullOrEmpty(user.DisplayName))
            {
                return RedirectToAction(nameof(Profile), new { statusMessage = "Error_DisplayName_Missing" });
            }

            try
            {
                IdentityResult identityResult = new();
                bool isChanged = false;
                if (usingIdentityUser.DisplayName != user.DisplayName)
                {
                    user.DisplayName = usingIdentityUser.DisplayName;
                    isChanged = true;
                }
                if (usingIdentityUser.Email != user.Email)
                {
                    user.Email = usingIdentityUser.Email;
                    isChanged = true;
                }

                if (isChanged)
                {
                    identityResult = await _userManager.UpdateAsync(user);

                    if (!identityResult.Succeeded)
                    {
                        trackEvents.TrackError("ProfileChange: DisplayName change failed" + identityResult.Errors
                            , new Dictionary<string, object> { { "IdentityeResult", identityResult } }, user.Id);
                        return RedirectToAction(nameof(Profile), new { statusMessage = "Error_Unknown" });
                    }
                }
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, nameof(ProfileChange), null, user.Id);
                return RedirectToAction(nameof(Profile), new { statusMessage = "Error_Unknown" });
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(Profile), new { statusMessage = "Success_Profile_Change", statusCode = 200 });
        }

        public async Task<IActionResult> DownloadPersonalData()
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Passkey", new { statusmessage = "Error_User_NotFound" });
            }

            Dictionary<string, string> personalData = [];
            IEnumerable<PropertyInfo> personalDataProps = typeof(UsingIdentityUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (PropertyInfo? p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
            foreach (UserLoginInfo l in logins)
            {
                personalData.Add(l.LoginProvider + " external login provider key.", l.ProviderKey);
            }

            Response.Headers.Append("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }

        public async Task<IActionResult> DeletePersonalData()
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Passkey", new { statusmessage = "Error_User_NotFound" });
            }

            return View();
        }

        [Authorize(Policy = "RequireRecentPasskey")]
        public async Task<IActionResult> DeletePersonalDataSubmit()
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Passkey", new { statusmessage = "Error_User_NotFound" });
            }

            try
            {
                List<CollectionItemDisplayDTO> collectionItemEntityList = processCollectionItemEntity.GetWithPredicates(new CollectionItemSearchParameterModel { UsingIdentityUsersID = [user.Id] });
                foreach (var collectionItem in collectionItemEntityList)
                {
                    processCollectionItemEntity.Delete(collectionItem.CollectionItemEntity.CollectionItemEntityID);
                }

                var credentiaList = processFidoCredential.GetCredentialsByUserId(user.Id);
                foreach (var credential in credentiaList)
                {
                    processFidoCredential.Delete(credential.CredentialId, user.Id);
                }

                IdentityResult result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return RedirectToAction(nameof(Profile), new { statusMessage = "UnexpectedError" + result.ToString() });
                }

                await _signInManager.SignOutAsync();
                if (user.Email != null)
                {
                    await emailSender.SendEmailAsync(
                        user.Email,
                        stringLocalizer["Success_Account_Deleted"],
                        stringLocalizer["Success_Account_Deleted"]);
                }
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, nameof(DeletePersonalDataSubmit), null);
                return RedirectToAction("Profile", "Home", new { statusMessage = "Error_Unknown" });
            }

            return RedirectToAction("Frontpage", "Home", new { statusMessage = "Success_Account_Deleted" });
        }
    }
}