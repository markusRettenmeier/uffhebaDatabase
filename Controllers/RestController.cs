using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Data;
using Sammlerplattform.Resources;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PartyProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Models.ImprovementSuggestions;
using Microsoft.AspNetCore.Identity;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;

namespace Sammlerplattform.Controllers
{
    [Route("api/collections")]
    public class RestController(
        IProcessEra processEra,
        IProcessPlace processPlace,
        IProcessParty processParty,
        IUnitOfWork unitOfWork,
        IProcessConcept processConcept,
        IProcessConceptRelation processConceptRelation,
        IProcessCollectionArea processCollectionArea,
        IStringLocalizer<SharedResources> stringLocalizer,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        IDeeplTranslationService translationService,
        UserManager<UsingIdentityUser> userManager) : Controller
    {
        [HttpPost("listPlaces")]
        public IActionResult ListPlaces([FromBody] PlaceSearchDTO placeSearchDTO)
        {
            PlaceSearchParameterModel model = new();
            List<PlaceDTO> placeDTOList = [];
            
            if (!string.IsNullOrEmpty(placeSearchDTO.Toponym))
            {
                model.PlaceNToponymyList_Toponymy_ToponymyName = [placeSearchDTO.Toponym];
            }            

            foreach (Place place in processPlace.GetListWithPredicate(model))
            {
                var vm = PlaceViewModelHelper.FromDomainModel(place);
                placeDTOList.Add(new PlaceDTO
                {
                    PlaceID = place.PlaceID,
                    ToponymyDisplay = vm.Toponymy,
                    FurtherSpecs = vm.FurtherSpecs
                });
            }

            return Ok(placeDTOList);
        }
        public class PlaceSearchDTO
        {
            public string? Toponym { get; set; }
        }
        public class PlaceDTO
        {
            public int PlaceID { get; set; }
            public string ToponymyDisplay { get; set; } = string.Empty;
            public string FurtherSpecs { get; set; } = string.Empty;
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
                    if (!string.IsNullOrWhiteSpace(x.Individual.Pseudonym)) 
                    { 
                        specs.Add(stringLocalizer["Pseudonym"] + ": " + x.Individual.Pseudonym); 
                    } 
                    if (!string.IsNullOrWhiteSpace(x.Individual.Signature)) 
                    { 
                        specs.Add("Signatur: " + x.Individual.Signature); 
                    } 
                }
                if (x.Organization != null)
                {
                    string? industry = x.Organization.Industry?.IndustryName;
                    if (!string.IsNullOrWhiteSpace(industry)) 
                    { 
                        specs.Add(stringLocalizer["Industry"] + ": " + industry); 
                    }
                }

                return new PartyDTO
                {
                    PartyID = x.PartyID,
                    Name = x.PartyName,
                    Type = x.PartyTypeEnum.GetDisplayName(),
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

        [HttpGet("listIndustries")]
        public IActionResult ListIndustries()
        {
            List<IndustryDTO> industryList = [.. unitOfWork.IndustryRepository.Get()
                .OrderBy(pf => pf.IndustryName)
                .Select(pf => new IndustryDTO
                {
                    Name = pf.IndustryName
                })];

            return Ok(industryList);
        }
        public class IndustryDTO
        {
            public string? Name { get; set; }
        }

        [HttpGet("listCollectionAreas")]
        public IActionResult ListCollectionAreas()
        {
            List<CollectionAreaDTO> collectionAreas = [.. processCollectionArea.GetListWithPredicate(new Models.CollectionAreaDatabase.CollectionAreaSearchParameterModel())
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
        public IActionResult ConceptualRelationship(int rootConceptId)
        {
            if (rootConceptId <= 0)
            {
                return BadRequest("Invalid collectionAreaID.");
            }

            List<NodeDTO> nodes = [.. unitOfWork.ConceptRepository.Get(filter: x => x.Id == rootConceptId || x.RootConceptID == rootConceptId)
                .Select(c =>
                {
                    return new NodeDTO
                    {
                        ID = c.Id,
                        Label = translationStore.GetTranslation(
                            nameof(Concept),
                            c.Id,
                            nameof(ConceptViewModel.Name),
                            translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    };
                })];

            List<EdgeDTO> edges = [.. processConceptRelation.GetByRootConceptID(rootConceptId)
                .Select(x => new EdgeDTO
                {
                    From = x.FromConceptID,
                    To = x.ToConceptID,
                    Label = x.RelationType.GetDisplayName(),
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
        public ActionResult ListConcepts(string? conceptName, int collectionAreaId)
        {
            ConceptualRelationshipSearchParameterModel searchParameter = new()
            {
                ConceptTypeInt = [0] // Bool
            };
            if (!string.IsNullOrEmpty(conceptName))
            {
                List<int> entityIds = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter { EntityType = [nameof(Concept)], TranslatedText = [conceptName] }).Select(x => x.EntityId)];
                searchParameter.Id = entityIds;
            }
            if (collectionAreaId > 0)
            {
                searchParameter.CollectionAreaID = [collectionAreaId];
            }
            List<ConceptualRelationshipOperationParameterModel> conceptialRelations = processConcept.Get(searchParameter);

            List<ConceptDTO> conceptDtoList = [.. processConcept.Get(searchParameter)
                .Select(x =>  
                {
                    List<string> specs = [];
                    if(x.ConceptViewModel.GetRootConceptId() != x.ConceptViewModel.Id)
                    {
                        specs.Add(stringLocalizer["RootConcept"] + ": " + processConcept
                            .Get(new ConceptualRelationshipSearchParameterModel {Id = [x.ConceptViewModel.GetRootConceptId()]})
                            .FirstOrDefault()?.ConceptViewModel.Name);
                    }
                    if(x.ConceptViewModel.SubConceptList.Count > 0)
                    {
                        specs.Add(stringLocalizer["SubConcepts"] + ": " + string.Join(", ", x.ConceptViewModel.SubConceptList.Select(sc => sc.Name)));
                    }
                    if(x.ConceptViewModel.Description != null)
                    {
                        specs.Add(stringLocalizer["Description"] + ": " + x.ConceptViewModel.Description);
                    }

                    return new ConceptDTO
                    {
                        ConceptID = x.ConceptViewModel.Id,
                        ConceptName = x.ConceptViewModel.Name,
                        FurtherSpecs = string.Join("; ", specs)
                    };
                })];

            return Ok(conceptDtoList);
        }

        public class ConceptDTO
        {
            public int ConceptID { get; set; }
            public string? ConceptName { get; set; }
            public string? FurtherSpecs { get; set; }
        }

        [HttpPost("VoteTopic")]
        public IActionResult VoteTopic([FromBody]VoteDTO voteDTO)
        {
            if(voteDTO == null || voteDTO.TopicId <= 0)
            {
                return BadRequest("Invalid topicId.");
            }

            TopicVote? existingVote = unitOfWork.TopicVoteRepository.Get(
                filter: tv => tv.UserId == userManager.GetUserId(User) && tv.TopicId == voteDTO.TopicId).FirstOrDefault();
            if (existingVote != null)
            {
                if (Enum.Parse<VoteType>(voteDTO.VoteType) == existingVote.VoteType)
                {
                    // If the same vote is cast again, remove the vote
                    unitOfWork.TopicVoteRepository.Delete(existingVote);
                }
                else
                {
                    existingVote.VoteType = Enum.Parse<VoteType>(voteDTO.VoteType);
                    existingVote.VotedAt = DateTime.UtcNow;
                }
            }
            else
            {
                unitOfWork.TopicVoteRepository.Insert(new TopicVote
                {
                    TopicId = voteDTO.TopicId,
                    UserId = userManager.GetUserId(User) ?? throw new NullReferenceException(),
                    VoteType = Enum.Parse<VoteType>(voteDTO.VoteType),
                    VotedAt = DateTime.UtcNow
                });
            }
            unitOfWork.Save();

            return Ok();
        }

        public class VoteDTO
        {
            public int TopicId { get; set; } 
            public required string VoteType { get; set; } // not public VoteType VoteType { get; set; }, cause can't fill it correctly and than in VoteTopic voteDTO is null
        }
    }
}
