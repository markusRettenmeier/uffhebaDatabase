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
                "Side_Bottom", "to", "Toponymy", "IsCurrentName", "ToponymyName", "EnterToponymy",
                "OwnershipProof_Type_BillOfSale", "OwnershipProof_Type_Certificate", "OwnershipProof_Type_Other",
                "Error_DisplayName_Missing", "Error_DisplayName_StringLength", "Error_Email_NotValid",
                "Error_Relationship_Required"
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