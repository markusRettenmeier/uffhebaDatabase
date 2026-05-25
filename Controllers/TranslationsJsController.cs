using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Resources;

namespace Sammlerplattform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationsJsController(IStringLocalizer<SharedResources> localizer) : ControllerBase
    {
        [HttpGet]
        public ActionResult<Dictionary<string, string>> Get()
        {
            var translations = new Dictionary<string, string>();

            var keys = new[]
            {
                "Details", "NothingFound", "Place_Add", "CookieConsent_Text ",
                "Concept_SynonymAdd", "Concept_SubTermAdd", "Add", "Column_Select",
                "Individual", "Organization", "Concept_ParentConceptAdd", "SubTermOf",
                "NumberRange_Change", "Remove", "Side_Front", "Side_Back", "Side_Left", "Side_Right", "Side_Top",
                "Side_Bottom", "Toponymy", "IsCurrentName", "ToponymyName", "EnterToponymy",
                "OwnershipProof_Type_BillOfSale", "OwnershipProof_Type_Certificate", "OwnershipProof_Type_Other",
                "Error_DisplayName_Missing", "Error_DisplayName_StringLength", "Error_Email_NotValid",
                "Error_Relationship_Required", "WebAuthn_Supported", "WebAuthn_Not_Supported", "WebAuthn_RegistrationSuccess", 
                "WebAuthn_AuthenticationSuccess", "Error_WaitingResponse", "Preparing", "Error_AssertionOptions_Ocurred",
                "Error_NoPasskeysFound", "Login_PleaseAuthenticate", "Login_Verifizing", "Error_VerifyAssertion_Ocurred",
                "Success_Login", "Login_Failed", "Error_Authentication_Ocurred", "Error_Registration_Ocurred",
                "Error_UserName_Required", "Error_DisplayName_Required", "Error_DisplayName_StringLength", "Error_Email_Invalid",
                "Error_Server_Error", "Success_Passkey_Registered", "Register_Redirect", "Register_Username_File_Content",
                "Error_Register_Aborted", "Register_Completing", "Error_PasskeyUserId_Missing", "Error_PasskeyChallenge_Ocurred"
                , "Error_PasskeyChallenge_Invalid", "Error_PasskeyChallenge_Expired", "Login_Success", "Error_Passkeys_NotFound",
                "Error_Authentication_Failed", "Error_Session_Expired", "Error_Credential_NotFound", "Error_Authentication_Cancelled",
                "Error_WebAuthn_NotSupported", "Error_Timeout", "Error_Network", "Error_Unknown", "Error_Security_HTTPS_Required"
            };

            foreach (var key in keys)
            {
                var value = localizer[key];
                if (!value.ResourceNotFound)
                {
                    translations[key] = value.Value;
                }
            }

            return translations;
        }
    }
}