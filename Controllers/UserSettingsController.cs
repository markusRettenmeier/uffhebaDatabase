using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class UserSettingsController(UserManager<UsingIdentityUser> userManager,
        SignInManager<UsingIdentityUser> signInManager,
        IEmailSender emailSender
        , IStringLocalizer<SharedResources> stringLocalizer
        , ITrackEvents trackEvents) : Controller
    {
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;
        private readonly SignInManager<UsingIdentityUser> _signInManager = signInManager;

        [HandleStatus]
        public IActionResult Profile()
        {
            UsingIdentityUser? user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_User_NotFound" });
            }

            return View(user);
        }

        public async Task<IActionResult> ProfileChange(UsingIdentityUser usingIdentityUser)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_Place_NotFound" });
            }
            try
            {
                if (usingIdentityUser.UserName != user.UserName)
                {
                    IdentityResult usernameResult = new();
                    if (string.IsNullOrEmpty(user.UserName))
                    {
                        usernameResult = await _userManager.SetUserNameAsync(user, usingIdentityUser.UserName);
                    }
                    else
                    {
                        user.UserName = usingIdentityUser.UserName;
                        usernameResult = await _userManager.UpdateAsync(user);
                    }

                    if (!usernameResult.Succeeded)
                    {
                        trackEvents.TrackWarning("ProfileChange: UserName change failed" + usernameResult.Errors, new Dictionary<string, object>
                            {{"UsernameResult", usernameResult} }, user.Id);
                        return RedirectToAction(nameof(Profile), new { statusMessage = "Error_UserName_Change" });
                    }
                }
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, nameof(ProfileChange), null, user.Id);
                return RedirectToAction(nameof(Profile), new { statusMessage = "Error_Error_Ocurred" });
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(Profile), new { statusMessage = "Success_Profile_Change", statusCode = 200 });
        }


        public async Task<IActionResult> DownloadPersonalData()
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_User_NotFound" });
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
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_User_NotFound" });
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
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_User_NotFound" });
            }

            bool RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Password))
                {
                    ModelState.AddModelError(string.Empty, "Error_Password_Incorrect");
                    return RedirectToAction(nameof(Profile));
                }
            }

            try
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return RedirectToAction(nameof(Profile), new { statusMessage = "UnexpectedError" + result.ToString() });
                }

                await _signInManager.SignOutAsync();
                statusMessage = "Löschung erfolgreich.";
                if (user.Email != null)
                {
                    await emailSender.SendEmailAsync(
                        user.Email,
                        stringLocalizer["SuccessAccountWasDeleted"],
                        stringLocalizer["SuccessAccountWasDeleted"]);
                }
            }
            catch (Exception ex)
            {
                statusMessage = "Error_Error_Ocurred";
                trackEvents.TrackException(ex, nameof(DeletePersonalDataSubmit), null);
            }

            return RedirectToAction("Frontpage", "Home", new { statusMessage });
        }

        public async Task<IActionResult> ChangePassword(string statusMessage)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_User_NotFound" });
            }

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

            try
            {
                if (changePasswordModel.OldPassword != null && changePasswordModel.NewPassword != null)
                {
                    IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordModel.OldPassword, changePasswordModel.NewPassword);
                    if (!changePasswordResult.Succeeded)
                    {
                        foreach (IdentityError error in changePasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                            statusMessage += error.Description;
                        }
                        return RedirectToAction(nameof(ChangePassword), new { statusMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, nameof(ChangePasswordSubmit), null, user.Id);
                return RedirectToAction(nameof(ChangePassword), new { statusMessage = "Error_Error_Ocurred" });
            }
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction(nameof(ChangePassword), new { statusMessage = "Success_Password_Change" });
        }

        public async Task<IActionResult> ChangeEMail(string statusMessage)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_User_NotFound" });
            }

            string? email = await _userManager.GetEmailAsync(user);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Profile), new { statusMessage = "Error_Email_Missing" });
            }
            ChangeEMailModel changeEMailModel = new()
            {
                OldEmail = email,
                IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
                HasPassword = await _userManager.HasPasswordAsync(user)
            };

            return View(changeEMailModel);
        }
        public async Task<IActionResult> ChangeEMailSubmit(ChangeEMailModel changeEMailModel)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_User_NotFound" });
            }

            bool RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword && changeEMailModel.Password != null)
            {
                if (!await _userManager.CheckPasswordAsync(user, changeEMailModel.Password))
                {
                    ModelState.AddModelError(string.Empty, "Error_Password_Incorrect");
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
                    string htmlBody = stringLocalizer["ConfirmAccountHtml", HtmlEncoder.Default.Encode(callbackUrl)].Value;
                    await emailSender.SendEmailAsync(
                        changeEMailModel.NewEmail,
                        stringLocalizer["Confirm your email"],
                        htmlBody);
                }

                return RedirectToAction(nameof(ChangeEMail), new { statusMessage = "Success_ConfirmationEmail_Sent", statusCode = 200 });
            }

            return RedirectToAction(nameof(ChangeEMail), new { statusMessage = "Error_Email_NotChanged" });
        }
    }
}