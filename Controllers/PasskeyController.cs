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
using Sammlerplattform.Services.Passkey;
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
        IDataProtectionProvider dataProtectionProvider,
        IProcessBackupCode processBackupCode) : Controller
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
                return BadRequest(new { error = stringLocalizer["Error_DisplayName_Missing"] });

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
                    return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
                }

                // Fido2User erstellen
                CreateFido2User(user, Request.Host.Host, out CredentialCreateOptions options, out string sessionKey);

                // Füge nach dem Speichern in Session hinzu
                HttpContext.Session.SetString("test", "working"); // Test-Eintrag

                return Ok(new
                {
                    success = true,
                    options,
                    sessionKey,
                    userId = user.Id,
                    userName = user.UserName
                });
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Error StartRegistration");
                return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> MakeCredential([FromBody] MakeCredentialRequest request)
        {
            var sessionData = GetFromSession<RegistrationSessionData>(request.SessionKey);
            if (sessionData == null)
            {
                trackEvents.TrackError("Session data not found for session key {SessionKey} during MakeCredential",
                    new Dictionary<string, object> { { "SessionKey", request.SessionKey } });

                // KEIN Benutzer-Löschen hier! Der Benutzer muss über sessionData.UserId gefunden werden
                return BadRequest(new { error = stringLocalizer["Error_Session_Expired"] });
            }

            // RICHTIG: User über sessionData.UserId finden, nicht über User.Identity
            var user = await userManager.FindByIdAsync(sessionData.UserId);
            if (user == null)
            {
                trackEvents.TrackError("User with ID {UserId} not found during MakeCredential",
                    new Dictionary<string, object> { { "UserId", sessionData.UserId } });

                // Lösche den Benutzer, wenn er nicht existiert? Nein, er existiert ja nicht.
                return BadRequest(new { error = stringLocalizer["Error_User_NotFound"] });
            }

            try
            {
                var result = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = request.AttestationResponse,
                    OriginalOptions = sessionData.Options,
                    IsCredentialIdUniqueToUserCallback = async (args, ct) =>
                    {
                        var existingCredential = processFidoCredential.GetCredentialById(args.CredentialId);
                        if (existingCredential != null)
                        {
                            trackEvents.TrackError("Credential ID {CredentialId} already exists",
                                new Dictionary<string, object> { { "CredentialId", args.CredentialId } });
                            return false;
                        }
                        return true;
                    }
                }, CancellationToken.None);

                if (result == null)
                {
                    await userManager.DeleteAsync(user);
                    trackEvents.TrackError("MakeNewCredentialAsync returned null or error status",
                        new Dictionary<string, object> { { "UserId", user.Id } });
                    return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
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

                if (statuscode != 201)
                {
                    await userManager.DeleteAsync(user);
                    trackEvents.TrackError("Storing credential failed: {StatusMessage}",
                        new Dictionary<string, object> { { "StatusMessage", statusMessage } }, user.Id);
                    return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
                }

                // Session aufräumen
                httpContextAccessor.HttpContext?.Session.Remove(request.SessionKey);

                // Benutzer anmelden
                await signInManager.SignInAsync(user, isPersistent: false);

                return Ok(new
                {
                    success = true,
                    message = stringLocalizer["Success_Passkey_Registered"]
                });
            }
            catch (Exception ex)
            {
                await userManager.DeleteAsync(user);
                trackEvents.TrackException(ex, "Error MakeCredential");
                return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
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
        public async Task<IActionResult> MakeAssertionOptions([FromBody] MakeAssertionOptionsRequest request)
        {
            try
            {
                List<PublicKeyCredentialDescriptor> allowedCredentials = [];
                string? username = request?.Username;
                bool hasCredentials = false;

                if (!string.IsNullOrEmpty(username))
                {
                    // Mit Username: Nur Credentials dieses Users erlauben
                    var user = await userManager.FindByNameAsync(username);
                    if (user != null)
                    {
                        allowedCredentials = GetCredentialsByUserId(user.Id);
                        hasCredentials = allowedCredentials.Count != 0;

                        if (!hasCredentials)
                        {
                            return Ok(new
                            {
                                success = false,
                                error = stringLocalizer["Error_PasskeyForUser_NotFound", username],
                                hasCredentials = false
                            });
                        }
                    }
                    else
                    {
                        return Ok(new
                        {
                            success = false,
                            error = stringLocalizer["Error_User_NotFound"],
                            hasCredentials = false
                        });
                    }
                }

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
                return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
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
                return BadRequest();
            }

            var options = JsonSerializer.Deserialize<AssertionOptions>(json)!;

            var credentialId = Base64UrlDecode(clientResponse.Assertion.Id);
            var credential = processFidoCredential.GetCredentialById(credentialId);
            if (credential == null)
            {
                trackEvents.TrackError("Credential with ID {CredentialId} not found during assertion verification", new Dictionary<string, object> { { "CredentialId", credentialId } });
                return Unauthorized();
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
                return BadRequest();
            }

            // 🔐 LOGIN ERFOLGREICH
            var user = await userManager.FindByIdAsync(credential.UserId);
            if (user == null)
            {
                trackEvents.TrackError("User with ID {UserId} not found during assertion verification", new Dictionary<string, object> { { "UserId", credential.UserId } });
                return BadRequest();
            }

            // Addditonal claims for DeleteAccount, to verify that the user recently authenticated with a passkey. This claim is checked in the DeleteAccount action and must be present and not older than 5 minutes.
            var claims = new List<Claim>
            {
                new("passkey_reverified_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
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
                return BadRequest(stringLocalizer["Error_Unknown"]);
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
            return RedirectToAction("Frontpage", "Home", new { StatusMessage = "Success_Logout", StatusCode = 200 });
        }

        private void CreateFido2User(UsingIdentityUser user, string host, out CredentialCreateOptions options, out string sessionKey)
        {
            try
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
                        //AuthenticatorAttachment = AuthenticatorAttachment.Platform,
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
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Error CreateFido2User");
                throw; // Fehler weiterwerfen, damit der Aufrufer (StartRegistration) den Fehler behandeln kann
            }
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
            if (string.IsNullOrEmpty(key))
            {
                return default;
            }
            var protectedData = httpContextAccessor.HttpContext?.Session.GetString(key);
            if (string.IsNullOrEmpty(protectedData))
            {
                return default;
            }

            try
            {
                var json = _dataProtector.Unprotect(protectedData);
                var result = JsonSerializer.Deserialize<T>(json);
                return result;
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, $"Error decrypting session data for key {key}");
                return default;
            }
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
                DeviceName = user.DisplayName
            };

            return processFidoCredential.Insert(fidoCredential);
        }

        public class MakeAssertionOptionsRequest
        {
            public string? Username { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GenerateBackupCodes()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Alte Backup-Codes löschen
            processBackupCode.DeleteRangeByUserId(user.Id);

            // 10 neue Backup-Codes generieren
            var plainCodes = BackupCodeService.GenerateBackupCodes(10, 8);
            var backupCodesToSave = new List<BackupCode>();

            foreach (var plainCode in plainCodes)
            {
                backupCodesToSave.Add(new BackupCode
                {
                    UserId = user.Id,
                    HashedCode = BackupCodeService.HashBackupCode(plainCode),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            // Neue speichern
            processBackupCode.InsertRange(backupCodesToSave);

            // Nur die Klartext-Codes zurückgeben (für den Nutzer, nur EINMAL!)
            return Ok(new { backupCodes = plainCodes });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyBackupCode([FromBody] VerifyBackupCodeRequest request)
        {
            // 1. Benutzer finden
            var user = await userManager.FindByNameAsync(request.Username);
            if (user == null)
                return BadRequest();

            // 3. Alle unbenutzten Backup-Codes des Benutzers laden
            var backupCodes = processBackupCode.GetByUserId(user.Id);

            BackupCode? validCode = null;
            // 4. Jeden Code verifizieren (wegen Salt kann man nicht direkt suchen)
            foreach (var code in backupCodes)
            {
                if (BackupCodeService.VerifyBackupCode(request.BackupCode, code.HashedCode))
                {
                    validCode = code;
                    break;
                }
            }
            if (validCode == null)
                return BadRequest();

            // 5. Code als verwendet markieren
            processBackupCode.MarkAsUsed(validCode.Id);

            // 6. Benutzer anmelden
            await signInManager.SignInAsync(user, isPersistent: false);
            return Ok();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> StartAdditionalPasskey()
        {
            try
            {
                // Aktuellen Benutzer holen
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    trackEvents.TrackError("Error StartAdditionalPasskey: User Not found", new Dictionary<string, object> { { "User", User } }, User.Identity?.Name);
                    return Unauthorized(new { error = stringLocalizer["Error_Unknown"] });
                }


                // Fido2User für den bestehenden Benutzer erstellen
                var guid = Guid.Parse(user.Id);
                var fido2User = new Fido2User
                {
                    Id = guid.ToByteArray(),
                    Name = user.UserName,
                    DisplayName = user.DisplayName ?? user.UserName
                };

                // Vorhandene Passkeys des Benutzers (um Duplikate zu vermeiden)
                var existingCredentials = GetCredentialsByUserId(user.Id);

                // Optionen für neuen Passkey erstellen
                var options = fido2.RequestNewCredential(new RequestNewCredentialParams
                {
                    User = fido2User,
                    ExcludeCredentials = existingCredentials, // Verhindert doppelte Registrierung
                    AttestationPreference = AttestationConveyancePreference.None,
                    AuthenticatorSelection = new AuthenticatorSelection
                    {
                        // Kein "Platform" - erlaubt sowohl Cloud als auch Hardware
                        ResidentKey = ResidentKeyRequirement.Required,
                        UserVerification = UserVerificationRequirement.Required
                    },
                    Extensions = new AuthenticationExtensionsClientInputs
                    {
                        CredProps = true
                    },
                    PubKeyCredParams = PubKeyCredParam.Defaults
                });
                options.Rp.Id = Request.Host.Host;
                options.Rp.Name = "uffheba";

                // Session speichern
                var sessionKey = StoreInSession(new RegistrationSessionData
                {
                    UserId = user.Id,
                    Options = options
                });

                return Ok(new
                {
                    success = true,
                    options,
                    sessionKey
                });
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Error StartAdditionalPasskey");
                return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPasskey([FromBody] MakeCredentialRequest request)
        {
            try
            {
                // DEBUG: Prüfen ob die AttestationResponse angekommen ist
                if (request.AttestationResponse == null)
                {
                    trackEvents.TrackError("AttestationResponse is null in AddPasskey",
                        new Dictionary<string, object> { { "SessionKey", request.SessionKey } });
                    return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
                }

                // Session-Daten holen
                var sessionData = GetFromSession<RegistrationSessionData>(request.SessionKey);
                if (sessionData == null)
                {
                    await signInManager.SignOutAsync();
                    trackEvents.TrackError("Session data not found for session key {SessionKey} during MakeCredential",
                        new Dictionary<string, object> { { "SessionKey", request.SessionKey } });
                    return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
                }

                // Benutzer holen
                var user = await userManager.FindByIdAsync(sessionData.UserId);
                if (user == null)
                {
                    await signInManager.SignOutAsync();
                    trackEvents.TrackError("User with ID {UserId} not found during MakeCredential",
                        new Dictionary<string, object> { { "UserId", sessionData.UserId } });
                    return Unauthorized(new { error = stringLocalizer["Error_Unknown"] });
                }

                // Credential verifizieren
                var result = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = request.AttestationResponse,
                    OriginalOptions = sessionData.Options,
                    IsCredentialIdUniqueToUserCallback = async (args, ct) =>
                    {
                        // Prüfen, ob die CredentialId bereits existiert
                        var existing = processFidoCredential.GetCredentialById(args.CredentialId);
                        return existing == null;
                    }
                }, CancellationToken.None);
                if (result == null)
                {
                    await signInManager.SignOutAsync();
                    trackEvents.TrackError("MakeNewCredentialAsync returned null or error status",
                        new Dictionary<string, object> { { "UserId", user.Id } });
                    return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
                }

                // Neuen Passkey speichern (ohne neuen Benutzer zu erstellen!)
                var (statusCode, statusMessage) = StoreCredential(
                    user.Id,
                    result.User,
                    result.Id,
                    result.PublicKey,
                    result.SignCount,
                    result.Type.ToString(),
                    result.AaGuid
                );
                if (statusCode != 201)
                {
                    await signInManager.SignOutAsync();
                    trackEvents.TrackError("StoreCredential returned null or error status",
                        new Dictionary<string, object> { { "UserId", user.Id } });
                    return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
                }

                // Session aufräumen
                httpContextAccessor.HttpContext?.Session.Remove(request.SessionKey);

                // Prüfen ob der Benutzer bereits Passkeys hatte
                var existingCount = processFidoCredential.GetCredentialsByUserId(user.Id).Count;

                return Ok(new
                {
                    success = true,
                    passkeyCount = existingCount,
                    requiresBackupCodes = existingCount == 1 // Erster Passkey? Dann Backup-Codes anbieten
                });
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Error AddPasskey");
                return BadRequest(new { error = stringLocalizer["Error_Unknown"] });
            }
        }
    }
}