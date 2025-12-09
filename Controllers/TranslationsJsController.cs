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
                "Details", "NothingFound", "Place_ParentAdd", "Place_ChildAdd", "RelatedGeography_Add", "Place_Add",
                "Concept_SynonymAdd", "Concept_SubTermAdd", "Concept_ShortForAdd", "Add", "Option_Select", "Field",
                "Region", "Relief", "Settlement", "TransportRoute", "BodyOfWater", "Building", "Individual", "Organization",
                "Company", "Institution", "Other", "NumberRange_Change", "Remove", "Color_Select", "IsPrimaryColor",
                "IsPrimaryMaterial", "Material_Select", "Side_Front", "Side_Back", "Side_Left", "Side_Right", "Side_Top", 
                "Side_Bottom", "to", "Postalcode", "IsCurrentPostalcode", "Toponymy", "IsCurrentName", "ToponymyName",
                "ParentPlace", "ChildPlace"
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