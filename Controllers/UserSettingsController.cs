using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Sammlerplattform.Resources;
using Sammlerplattform.Models.UserSettings;
using System.Linq.Dynamic.Core;
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
        , IStringLocalizer<SharedResources> stringLocalizer) : Controller
    {
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;
        private readonly SignInManager<UsingIdentityUser> _signInManager = signInManager;

        public IActionResult Profile(string statusMessage, int statusCode)
        {
            var translatedMessage = stringLocalizer[statusMessage];
            if(translatedMessage.ResourceNotFound == false)
            {
                ViewData["StatusMessage"] = stringLocalizer[statusMessage];
            }
            else
            {
                ViewData["StatusMessage"] = statusMessage;
            }
            ViewData["StatusCode"] = statusCode;

            var userID = _userManager.GetUserId(User);
            if (userID == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_Place_NotFound" });
            }

            return View(User);
        }

        public async Task<IActionResult> ProfileChange(UsingIdentityUser usingIdentityUser)
        {
            UsingIdentityUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { statusmessage = "Error_Place_NotFound" });
            }

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
                    return RedirectToAction(nameof(Profile), new { statusMessage = "Error_UserName_Change" });
                }
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
            IEnumerable<System.Reflection.PropertyInfo> personalDataProps = typeof(UsingIdentityUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (System.Reflection.PropertyInfo? p in personalDataProps)
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
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

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
                scope.Complete();
            }
            catch (Exception ex)
            {
                statusMessage = "UnexpectedError" + ex.Message + ex.InnerException;
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

            ViewData["StatusMessage"] = stringLocalizer[statusMessage];

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
                    }
                    return RedirectToAction(nameof(ChangePassword), new { statusMessage });
                }
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

            ViewData["StatusMessage"] = stringLocalizer[statusMessage];
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