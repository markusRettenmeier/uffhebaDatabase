using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Models.Passkey;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PasskeyProcessees;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Sammlerplattform.Controllers
{
    [Route("[controller]/[action]")]
    public class PasskeyController(
        Fido2 fido2,
        IStringLocalizer<SharedResources> stringLocalizer,
        UserManager<UsingIdentityUser> userManager,
        SignInManager<UsingIdentityUser> signInManager,
        ITrackEventsCSV trackEvents,
        IProcessFidoCredential processFidoCredential,
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider) : Controller
    {
        private readonly IDataProtector _dataProtector = dataProtectionProvider.CreateProtector("Fido2");

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> StartRegistration([FromBody] StartRegistrationRequest request)
        {
            // Validierung
            if (string.IsNullOrEmpty(request.DisplayName))
                return BadRequest(stringLocalizer["Error_DisplayName_Missing"]);

            try
            {
                UsingIdentityUser user = new()
                {
                    UserName = Guid.NewGuid().ToString(), //Nur intern benötigt für Identity, da Login über Passkey erfolgt. Deshalb generieren wir hier einen zufälligen Wert.
                    DisplayName = request.DisplayName,
                    Email = request.Email
                };
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    trackEvents.TrackError("User creation failed: ", new Dictionary<string, object> { { "Errors", result.Errors } }, user.Id);
                    return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
                }

                // Fido2User erstellen
                CreateFido2User(user, Request.Host.Host, out CredentialCreateOptions options, out string sessionKey);

                return Ok(new
                {
                    success = true,
                    options,
                    sessionKey,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Error StartRegistration");
                return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> MakeCredential([FromBody] MakeCredentialRequest request)
        {
            var sessionData = GetFromSession<RegistrationSessionData>(request.SessionKey);
            if (sessionData == null)
            {
                var userToDelete = await userManager.GetUserAsync(User);
                if (userToDelete != null)
                {
                    await userManager.DeleteAsync(userToDelete);
                }
                trackEvents.TrackError("Session data not found for session key {SessionKey} during MakeCredential", new Dictionary<string, object> { { "SessionKey", request.SessionKey } });
                return BadRequest(stringLocalizer["Error_Session_Expired"]);
            }

            var user = await userManager.FindByIdAsync(sessionData.UserId);
            if (user == null)
            {
                trackEvents.TrackError("User with ID {UserId} not found during MakeCredential", new Dictionary<string, object> { { "UserId", sessionData.UserId } });
                return BadRequest(stringLocalizer["Error_User_NotFound"]);
            }

            try
            {
                var result = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = request.AttestationResponse,
                    OriginalOptions = sessionData.Options,                    
                    IsCredentialIdUniqueToUserCallback = async (args, ct) =>
                    {
                        try
                        {
                            var existingCredential = processFidoCredential.GetCredentialById(args.CredentialId);
                            if (existingCredential != null)
                            {
                                trackEvents.TrackError("Credential ID {CredentialId} already exists for user {UserId}",
                                    new Dictionary<string, object>  { { "args", args } } ,
                                    existingCredential.UserId);
                                return false;
                            }

                            return true;
                        }
                        catch (Exception ex)
                        {
                            trackEvents.TrackException(ex, "Error in IsCredentialIdUniqueToUserCallback");
                            return false;
                        }
                    }
                }, CancellationToken.None);
                if (result == null)
                {
                    await userManager.DeleteAsync(user);
                    return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
                }

                (int statuscode, string statusMessage) = StoreCredential(
                    user.Id,
                    result.User,
                    result.Id,
                    result.PublicKey,
                    result.SignCount,
                    result.Type.ToString(),
                    result.AaGuid
                );
                if(statuscode != 201)
                {
                    await userManager.DeleteAsync(user);
                    trackEvents.TrackError("Storing credential failed: {StatusMessage}", new Dictionary<string, object> { { "StatusMessage", statusMessage } }, user.Id);
                    return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
                }

                // Session aufräumen
                httpContextAccessor.HttpContext?.Session.Remove(request.SessionKey);

                // Bei neuer Benutzerregistrierung direkt anmelden
                if (!User.Identity?.IsAuthenticated == true)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                }

                return Ok(new
                {
                    success = true,
                    message = "Passkey erfolgreich registriert!",
                    redirectUrl = Url.Action("Login", "Passkey")
                });
            }
            catch (Exception ex)
            {
                await userManager.DeleteAsync(user);
                trackEvents.TrackException(ex, "Error MakeCredential");
                return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> MakeAssertionOptions()
        {
            try
            {
                var options = fido2.GetAssertionOptions(
                    [],
                    UserVerificationRequirement.Required,
                    new AuthenticationExtensionsClientInputs()
                );

                var sessionKey = $"fido2.assertion.{Guid.NewGuid()}";
                HttpContext.Session.SetString(
                    sessionKey,
                    JsonSerializer.Serialize(options)
                );

                return Ok(new
                {
                    success = true,
                    options,
                    sessionKey
                });
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "MakeAssertionOptions");
                return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
            }
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAssertion([FromBody] VerifyAssertionRequest clientResponse)
        {
            var json = HttpContext.Session.GetString(clientResponse.SessionKey);
            if (json == null)
            {
                trackEvents.TrackError("Session key {SessionKey} not found for assertion verification", new Dictionary<string, object> { { "SessionKey", clientResponse.SessionKey } });
                return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
            }

            var options = JsonSerializer.Deserialize<AssertionOptions>(json)!;

            var credentialId = Base64UrlDecode(clientResponse.Assertion.Id);
            var credential = processFidoCredential.GetCredentialById(credentialId);
            if (credential == null)
            {
                trackEvents.TrackError("Credential with ID {CredentialId} not found during assertion verification", new Dictionary<string, object> { { "CredentialId", credentialId } });
                return Unauthorized(stringLocalizer["Error_Error_Ocurred"]);
            }

            var result = await fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = clientResponse.Assertion,
                OriginalOptions = options,
                StoredPublicKey = credential.PublicKey,
                StoredSignatureCounter = (uint)credential.SignatureCounter,
                IsUserHandleOwnerOfCredentialIdCallback = async (args, cancellationToken) =>
                {
                    var storedCredential = processFidoCredential.GetCredentialById(args.CredentialId);
                    if (storedCredential == null)
                        return false;

                    // UserHandle vergleichen
                    var guid = Guid.Parse(storedCredential.UserId);
                    var userHandle = guid.ToByteArray();
                    return userHandle.SequenceEqual(args.UserHandle);
                }
            });
            credential.SignatureCounter = result.SignCount;
            (int Statuscode, string Statusmessage) = processFidoCredential.UpdateSignatureCounter(credential.CredentialId, credential.SignatureCounter);
            if (Statuscode != 200)
            {
                trackEvents.TrackError("Failed to update signature counter for credential ID {CredentialId}: {StatusMessage}", new Dictionary<string, object> { { "CredentialId", credential.CredentialId }, { "StatusMessage", Statusmessage } });
                return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
            }

            // 🔐 LOGIN ERFOLGREICH
            var user = await userManager.FindByIdAsync(credential.UserId);
            if (user == null)
            {
                trackEvents.TrackError("User with ID {UserId} not found during assertion verification", new Dictionary<string, object> { { "UserId", credential.UserId } });
                return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
            }

            // Addditonal claims for DeleteAccount, to verify that the user recently authenticated with a passkey. This claim is checked in the DeleteAccount action and must be present and not older than 5 minutes.
            var claims = new List<Claim>
            {
                new("passkey_reverified_at",
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };
            await signInManager.SignInWithClaimsAsync(
                user,
                isPersistent: false,
                additionalClaims: claims);

            return Ok();
        }
        private static byte[] Base64UrlDecode(string base64Url)
        {
            // Convert Base64Url to Base64
            var base64 = base64Url
                .Replace('-', '+')
                .Replace('_', '/');

            // Add padding if needed
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return Convert.FromBase64String(base64);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Manage()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteCredential([FromBody] DeleteCredentialRequest request)
        {
            try
            {
                var userId = userManager.GetUserId(User);
                if (userId == null)
                    return Unauthorized();

                (int Statuscode, string Statusmessage) = processFidoCredential.Delete(request.CredentialId, userId);
                return Ok(new
                {
                    success = Statuscode == 200,
                    message = Statusmessage
                });
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Error deleting credential");
                return BadRequest(stringLocalizer["Error_Error_Ocurred"]);
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetCredentials()
        {
            var userId = userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var credentials = processFidoCredential.GetCredentialsByUserId(userId);
            return Ok(new
            {
                success = true,
                credentials = credentials.Select(c => new
                {
                    c.Id,
                    CredentialId = Convert.ToBase64String(c.CredentialId),
                    c.DeviceName,
                    c.RegDate,
                    RegDateFormatted = c.RegDate.ToString("dd.MM.yyyy HH:mm")
                })
            });
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Frontpage", "Home", new { Message = "Success_Logout", Code = 200 });
        }

        private void CreateFido2User(UsingIdentityUser user, string host, out CredentialCreateOptions options, out string sessionKey)
        {
            var guid = Guid.Parse(user.Id);
            var fido2User = new Fido2User
            {
                Id = guid.ToByteArray(),
                Name = user.UserName,
                DisplayName = user.DisplayName
            };
            // Existierende Credentials holen
            var existingCredentials = GetCredentialsByUserId(user.Id);

            // Options erstellen
            options = fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fido2User,
                ExcludeCredentials = existingCredentials,
                AttestationPreference = AttestationConveyancePreference.None,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    AuthenticatorAttachment = AuthenticatorAttachment.Platform,
                    ResidentKey = ResidentKeyRequirement.Required,
                    UserVerification = UserVerificationRequirement.Required
                },
                Extensions = new AuthenticationExtensionsClientInputs
                {
                    CredProps = true
                },
                PubKeyCredParams = PubKeyCredParam.Defaults
            });
            options.Rp.Id = host;
            options.Rp.Name = "uffheba";

            // Session Key speichern
            sessionKey = StoreInSession(new RegistrationSessionData
            {
                UserId = user.Id,
                Options = options,
                Username = user.UserName ?? "Unknown"
            });
        }

        private List<PublicKeyCredentialDescriptor> GetCredentialsByUserId(string userId)
        {
            var credentials = processFidoCredential.GetCredentialsByUserId(userId);
            return [.. credentials.Select(c => new PublicKeyCredentialDescriptor(
                type: PublicKeyCredentialType.PublicKey,
                id: c.CredentialId,
                transports: null
            ))];
        }
        private string StoreInSession(object data)
        {
            var key = Guid.NewGuid().ToString();
            var json = JsonSerializer.Serialize(data);
            var protectedData = _dataProtector.Protect(json);

            httpContextAccessor.HttpContext?.Session.SetString(key, protectedData);
            return key;
        }
        private T? GetFromSession<T>(string key)
        {
            var protectedData = httpContextAccessor.HttpContext?.Session.GetString(key);
            if (string.IsNullOrEmpty(protectedData))
                return default;

            var json = _dataProtector.Unprotect(protectedData);
            return JsonSerializer.Deserialize<T>(json);
        }

        private (int Statuscode, string Statusmessage) StoreCredential(string userId, Fido2User user, byte[] credentialId, byte[] publicKey, uint signCount, string? credType, Guid aaguid)
        {
            var fidoCredential = new FidoCredential
            {
                UserId = userId,
                CredentialId = credentialId,
                PublicKey = publicKey,
                SignatureCounter = signCount,
                CredType = credType,
                RegDate = DateTime.UtcNow,
                AaGuid = aaguid,
                DeviceName = user.DisplayName ?? "Unknown Device"
            };

            return processFidoCredential.Insert(fidoCredential);
        }
    }    
}