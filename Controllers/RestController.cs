using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Data;
using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PartyProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using Sammlerplattform.Models.Translations;

namespace Sammlerplattform.Controllers
{
    //[AllowAnonymous]
    [Route("api/collections")]
    public class RestController(
        IProcessEra processEra,
        IProcessPlace processPlace,
        IProcessParty processParty,
        IUnitOfWork unitOfWork,
        IProcessConcept processConcept,
        IProcessConceptRelation processConceptRelation,
        IProcessCollectionItemPotential processCollectionItemPotential,
        IStringLocalizer<SharedResources> stringLocalizer,
        IProcessTranslations processTranslations
        ) : Controller
    {

        [HttpPost("listPlaces")]
        public IActionResult ListPlaces([FromBody] PlaceSearchDTO placeSearchDTO)
        {
            PlaceSearchParameter model = new();
            if (placeSearchDTO != null)
            {
                if (placeSearchDTO.Toponym != null)
                {
                    List<int> entityIds = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter { EntityType = [nameof(Place)], TranslatedText = [placeSearchDTO.Toponym] }).Select(x => x.EntityId)];
                    if (entityIds.Count > 0)
                    {
                        model.PlaceID.AddRange(entityIds);
                    }
                }
                if (!string.IsNullOrEmpty(placeSearchDTO.Toponym))
                {
                    model.PlaceNToponymyList_Toponymy_ToponymyName = [placeSearchDTO.Toponym];
                }
                if (placeSearchDTO.ToponymyType != null)
                {
                    model.ToponymyTypeInt = [(int)placeSearchDTO.ToponymyType];
                }
            }
            List<Place> placeList = processPlace.GetListWithPredicate(model);

            List<PlaceDTO> placeDTOList = [.. placeList.Select(x =>
            {
                List<string> oeconymParts = x.PlaceNToponymyList?
                    .Select(t =>
                    {
                        string name = t.Toponymy?.ToponymyName ?? "";
                        return t.IsCurrentName
                            ? $"<strong>{name}</strong>"
                            : name;
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList() ?? [];

                string oeconymDisplay = string.Join(", ", oeconymParts);

                // 2. FurtherSpecs: PLZ, Beiname, Geografie
                List<string> specs = [];

                if (x.Settlement != null)
                {
                    List<string> currentPostalcodeList = [.. x.Settlement.SettlementNPostalcodeList
                        .Where(y => y.IsCurrentPostalcode)
                        .Select(y => y.Postalcode.PostalcodeNumber)];

                    if (currentPostalcodeList.Count != 0)
                    {
                        specs.Add(stringLocalizer["Postalcode"] + ": " + string.Join(", ", currentPostalcodeList));
                    }

                    if (!string.IsNullOrWhiteSpace(x.Settlement.Byname))
                    {
                        specs.Add(stringLocalizer["Epithet"] + ": " + x.Settlement.Byname);
                    }

                    if (x.Settlement.RelatedGeography != null)
                    {
                        specs.Add(stringLocalizer["Geography"] + ": " + x.Settlement.RelatedGeography.PlaceNToponymyList
                            .FirstOrDefault(x => x.IsCurrentName)?.Toponymy.ToponymyName);
                    }
                }

                if (x.ParentPlace != null)
                {
                    string? parentName = x.ParentPlace.PlaceNToponymyList?
                        .FirstOrDefault(t => t.IsCurrentName)?.Toponymy?.ToponymyName;

                    if (!string.IsNullOrWhiteSpace(parentName))
                    {
                        specs.Add(stringLocalizer["Part of"] + ": " + parentName);
                    }
                }

                return new PlaceDTO
                {
                    PlaceID = x.PlaceID,
                    OeconymDisplay = oeconymDisplay,
                    ToponymyType = EnumExtensions.GetDescription(x.ToponymyTypeEnum),
                    FurtherSpecs = string.Join("; ", specs)
                };
            })];

            return Ok(placeDTOList);
        }
        public class PlaceSearchDTO
        {
            public string? Toponym { get; set; }
            public int? ToponymyType { get; set; }
        }
        public class PlaceDTO
        {
            public int PlaceID { get; set; }
            public string OeconymDisplay { get; set; } = "";
            public string? ToponymyType { get; set; }
            public string FurtherSpecs { get; set; } = "";
        }

        [HttpPost("listParties")]
        public IActionResult ListParties([FromBody] PartySearchDTO partySearchDTO)
        {  
            PartySearchParameterModel model = new();
            if (partySearchDTO != null)
            {
                if (partySearchDTO.Name != null)
                {
                    List<int> entityIds = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter { EntityType = [nameof(Party)], TranslatedText = [partySearchDTO.Name] }).Select(x => x.EntityId)];
                    if (entityIds.Count > 0)
                    {
                        model.PartyID = entityIds;
                    }
                }
                if (!string.IsNullOrEmpty(partySearchDTO.Name))
                {
                    model.PartyName = [partySearchDTO.Name];
                }
                if (partySearchDTO.Type != null)
                {
                    model.PartyTypeInt = [(int)partySearchDTO.Type];
                }
            }
            List<Party> partyList = processParty.GetListWithPredicate(model);

            List<PartyDTO> partyDTOList = [.. partyList.Select(x =>
            {
                List<string> specs = [];
                if (x.Individual != null)
                {
                    if (!string.IsNullOrWhiteSpace(x.Individual.Pseudonym)) { specs.Add(stringLocalizer["Pseudonym"] + ": " + x.Individual.Pseudonym); } if (!string.IsNullOrWhiteSpace(x.Individual.Signature)) { specs.Add("Signatur: " + x.Individual.Signature); } }
                if (x.Organization != null)
                {
                    string? productionFacility = x.Organization.ProductionFacility?.ProductionFacilityName;
                    if (!string.IsNullOrWhiteSpace(productionFacility)) { specs.Add(stringLocalizer["ProductionFacility"] + ": " + productionFacility); } specs.Add("Organisationstyp: " + EnumExtensions.GetDescription(x.Organization.OrganizationTypeEnum));
                }

                return new PartyDTO
                {
                    PartyID = x.PartyID,
                    Name = x.PartyName,
                    Type = EnumExtensions.GetDescription(x.PartyTypeEnum),
                    FurtherSpecs = string.Join("; ", specs)
                };
            })];

            return Ok(partyDTOList);
        }
        public class PartySearchDTO
        {
            public string? Name { get; set; }
            public int? Type { get; set; }
        }
        public class PartyDTO
        {
            public int PartyID { get; set; }
            public string Name { get; set; } = "";
            public string? Type { get; set; }
            public string FurtherSpecs { get; set; } = "";
        }

        [HttpGet("listEras")]
        public IActionResult ListEras(string? name)
        {

            EraSearchParameterModel eraSearchParameter = new();
            if (!string.IsNullOrEmpty(name))
            {
                eraSearchParameter.EraName.Add(name);
                List<int> entityIds = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter { EntityType = [nameof(Era)], TranslatedText = [name] }).Select(x => x.EntityId)];
                if (entityIds.Count > 0)
                {
                    eraSearchParameter.EraID = entityIds;
                }
            }

            List<EraDTO> eraList = [.. processEra.GetWithPredicates(eraSearchParameter)
                .OrderBy(x => x.EraName)
                .Select(x => new EraDTO()
                {
                    EraID = x.EraID,
                    EraName = x.EraName
                })];

            return Ok(eraList);
        }
        public class EraDTO
        {
            public int EraID { get; set; }
            public string? EraName { get; set; }
        }

        [HttpGet("listColors")]
        public IActionResult ListColors()
        {
            List<ColorDTO> colorList = [.. unitOfWork.ColorRepository.Get()
                .OrderBy(x => x.Name)
                .Select(x => new ColorDTO
                {
                    ColorID = x.ColorID,
                    ColorName = x.Name
                })];

            return Ok(colorList);
        }
        public class ColorDTO
        {
            public int ColorID { get; set; }
            public string? ColorName { get; set; }
        }

        [HttpGet("listMaterials")]
        public IActionResult ListMaterials()
        {
            List<MaterialDTO> materialList = [.. unitOfWork.MaterialRepository.Get()
                .OrderBy(x => x.MaterialName)
                .Select(x => new MaterialDTO{
                    MaterialID = x.MaterialID,
                    Name = x.MaterialName
                })];
            return Ok(materialList);
        }
        public class MaterialDTO
        {
            public int MaterialID { get; set; }
            public string? Name { get; set; }
        }

        [HttpGet("listProductionFacilities")]
        public IActionResult ListProductionFacilities()
        {
            List<ProducitonFacilityDTO> productionFacilities = [.. unitOfWork.ProductionFacilityRepository.Get()
                .OrderBy(pf => pf.ProductionFacilityName)
                .Select(pf => new ProducitonFacilityDTO
                {
                    ID = pf.ProductionFacilityID,
                    Name = pf.ProductionFacilityName
                })];

            return Ok(productionFacilities);
        }
        public class ProducitonFacilityDTO
        {
            public int ID { get; set; }
            public string? Name { get; set; }
        }
        public class KeywordDTO
        {
            public int KeywordID { get; set; }
            public string? Name { get; set; }
        }

        [HttpGet("listCollectionAreas")]
        public IActionResult ListCollectionAreas()
        {
            List<CollectionAreaDTO> collectionAreas = [.. unitOfWork.CollectionAreaRepository.Get()
                .OrderBy(ca => ca.CollectionAreaName)
                .Select(ca => new CollectionAreaDTO
                {
                    ID = ca.CollectionAreaID,
                    Name = ca.CollectionAreaName
                })];
            return Ok(collectionAreas);
        }
        public class CollectionAreaDTO
        {
            public int ID { get; set; }
            public string? Name { get; set; }
        }

        [HttpGet("conceptualRelationship")]
        public IActionResult ConceptualRelationship(int collectionAreaID)
        {
            if (collectionAreaID <= 0)
            {
                return BadRequest("Invalid collectionAreaID.");
            }

            List<NodeDTO> nodes = [.. processConcept.GetWithPredicates(new ConceptualRelationshipSearchParameterModel()
            {
                CollectionAreaID = [collectionAreaID]
            })
                .Select(c => new NodeDTO
                {
                    ID = c.Concept.Id,
                    Label = c.Concept.ConceptName
                })];

            List<EdgeDTO> edges = [.. processConceptRelation.GetByCollectionAreaID(collectionAreaID)
                .Select(x => new EdgeDTO
                {
                    From = x.FromConceptID,
                    To = x.ToConceptID,
                    Label = x.RelationType.ToString(),
                    Arrows = x.IsDirected ? stringLocalizer["to"] : ""
                })];

            var result = new
            {
                Nodes = nodes,
                Edges = edges
            };

            return Ok(result);
        }
        public class NodeDTO()
        {
            public int ID { get; set; }
            public string? Label { get; set; }
        }
        public class EdgeDTO()
        {
            public int From { get; set; }
            public int To { get; set; }
            public string? Label { get; set; }
            public string? Arrows { get; set; }
        }

        [HttpGet("listConcepts")]
        public ActionResult ListConcepts(string? conceptName)
        {
            ConceptualRelationshipSearchParameterModel searchParameter = new();
            if (!string.IsNullOrEmpty(conceptName))
            {
                List<int> entityIds = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter { EntityType = [nameof(Concept)], TranslatedText = [conceptName] }).Select(x => x.EntityId)];
                searchParameter.ConceptID = entityIds;
                searchParameter.ConceptName = [conceptName];
            };

            List<ConceptDTO> conceptialRelations = [.. processConcept.GetWithPredicates(searchParameter)
                .Select(x => new ConceptDTO
                {
                    ConceptID = x.Concept.Id,
                    ConceptName = x.Concept.ConceptName,
                    Description = x.Concept.Description
                })];

            return Ok(conceptialRelations);
        }

        public class ConceptDTO
        {
            public int ConceptID { get; set; }
            public string? ConceptName { get; set; }
            public string? Description { get; set; }
        }

        [HttpPost("listPotentials")]
        public IActionResult ListPotentials([FromBody] CollectionItemPotentialSearchDTO searchDTO)
        {
            try
            {
                var searchParameter = new CollectionItemSearchParameterModel();

                if (searchDTO != null)
                {
                    if (searchDTO.PotentialID != null)
                        searchParameter.CollectionItemEntityID = [searchDTO.PotentialID.Value];

                    if (searchDTO.CollectionAreaID != null)
                        searchParameter.CollectionAreaID = [searchDTO.CollectionAreaID.Value];

                    if (searchDTO.UsingIdentityUsersID != null)
                        searchParameter.UsingIdentityUsersID = [searchDTO.UsingIdentityUsersID];
                }

                List<CollectionItemPotential> potentials = processCollectionItemPotential.GetWithPredicates(searchParameter);

                var collectionItemPotentialDTOList = potentials.Select(p =>
                {
                    // Hole das führende (Frontside) Bild direkt aus dem geladenen Include
                    var leadingPicture = p.CollectionItemEntityList?
                        .SelectMany(ci => ci.CollectionItemPictureList)
                        .FirstOrDefault(pic => pic.Frontside);

                    return new CollectionItemPotentialDTO
                    {
                        CollectionItemPotentialID = p.CollectionItemPotentialID,
                        LeadingPictureID = leadingPicture?.CollectionItemPictureID ?? 0
                    };
                }).ToList();

                return Ok(collectionItemPotentialDTOList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        public class CollectionItemPotentialSearchDTO
        {
            public int? PotentialID { get; set; }
            public int? CollectionAreaID { get; set; }
            public string? UsingIdentityUsersID { get; set; }
        }
        public class CollectionItemPotentialDTO
        {
            public int CollectionItemPotentialID { get; set; }
            public int LeadingPictureID { get; set; }
        }
    }
}
