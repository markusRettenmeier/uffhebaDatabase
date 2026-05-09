using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ImprovementSuggestions;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using Sammlerplattform.Services.DatabaseProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using Sammlerplattform.Services.Extensions;

namespace Sammlerplattform.Controllers
{
    [Route("api/collections")]
    public class RestController(
        IProcessEra processEra,
        IProcessPlace processPlace,
        IProcessParticpant processParticpant,
        IUnitOfWork unitOfWork,
        IProcessConcept processConcept,
        IProcessCollectionArea processCollectionArea,
        IStringLocalizer<SharedResources> stringLocalizer,
        IProcessTranslations processTranslations,
        UserManager<UsingIdentityUser> userManager,
        IProcessCIRelationship processCppRelationship) : Controller
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
        public IActionResult ListParties([FromBody] ParticipantSearchDTO participantSearchDTO)
        {
            ParticipantSearchParameterModel model = new();
            if (participantSearchDTO != null)
            {
                if (participantSearchDTO.Name != null)
                {
                    List<int> entityIds = [.. processTranslations.GetWithFallback(
                        new EntityTranslationSearchParameter
                        {
                            EntityType = [nameof(Participant)]
                            , TranslatedText = [participantSearchDTO.Name]
                        }).Select(x => x.EntityId)];
                    if (entityIds.Count > 0)
                    {
                        model.ParticipantID = entityIds;
                    }
                }
                if (!string.IsNullOrEmpty(participantSearchDTO.Name))
                {
                    model.ParticipantName = [participantSearchDTO.Name];
                }
                if (participantSearchDTO.Type != null)
                {
                    model.ParticipantTypeInt = [(int)participantSearchDTO.Type];
                }
            }
            List<Participant> participantList = processParticpant.GetListWithPredicate(model);

            List<ParticipantDTO> participantDTOList = [.. participantList.Select(x =>
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
                        specs.Add(stringLocalizer["Signature"] + ": " + x.Individual.Signature);
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

                return new ParticipantDTO
                {
                    ParticpantID = x.ParticipantID,
                    Name = x.ParticipantName,
                    Type = x.ParticipantTypeEnum.GetDisplayName(),
                    FurtherSpecs = string.Join("; ", specs)
                };
            })];

            return Ok(participantDTOList);
        }
        public class ParticipantSearchDTO
        {
            public string? Name { get; set; }
            public int? Type { get; set; }
        }
        public class ParticipantDTO
        {
            public int ParticpantID { get; set; }
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
                List<int> entityIds = [.. processTranslations.GetWithFallback(new EntityTranslationSearchParameter { EntityType = [nameof(Era)], TranslatedText = [name] }).Select(x => x.EntityId)];
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

            List<ConceptDisplayDTO> displayDTOList = processConcept.Get(new ConceptualRelationshipSearchParameterModel
            {
                RootConceptID = [rootConceptId]
            });
            List<NodeDTO> nodes = [.. displayDTOList
                .Select(c =>
                    new NodeDTO
                    {
                        ID = c.ConceptViewModel.Id,
                        Label = c.ConceptViewModel.Name
                    })];
            List<EdgeDTO> edges = [.. displayDTOList.SelectMany(x => x.ConceptRelationViewList.Select(cr => new EdgeDTO
            {
                From = cr.FromConceptID,
                To = cr.ToConceptID,
                Label = cr.RelationType.GetDisplayName(),
                Arrows = cr.IsDirected ? stringLocalizer["to"] : ""
            }))];
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
        public ActionResult ListConcepts(string? conceptName, int collectionAreaId, int rootConceptId)
        {
            ConceptualRelationshipSearchParameterModel searchParameter = new()
            {
                ConceptTypeInt = [0] // Bool
            };
            if (!string.IsNullOrEmpty(conceptName))
            {
                List<int> entityIds = [.. processTranslations.GetWithFallback(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(Concept)],
                    TranslatedText = [conceptName]
                }).Select(x => x.EntityId)];
                searchParameter.Id = entityIds;
                searchParameter.RootConceptID = [.. entityIds.Select(i => (int?)i)];
            }
            if (collectionAreaId > 0)
            {
                searchParameter.CollectionAreaID = [collectionAreaId];
            }
            if (rootConceptId > 0)
            {
                searchParameter.RootConceptID = [rootConceptId];
            }

            List<ConceptDTO> conceptDtoList = [.. processConcept.Get(searchParameter)
                .Where(x => x.ConceptViewModel.RootConceptID != null) // RootConcept is not listed, but can be specified as filter
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
        public IActionResult VoteTopic([FromBody] VoteDTO voteDTO)
        {
            if (voteDTO == null || voteDTO.TopicId <= 0)
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

        [HttpGet("listCIRelationships")]
        public IActionResult ListCIRelationships()
        {
            List<CollectionItemRelationship> relationships = processCppRelationship.GetListWithPredicates(new CIRelationshipSearchParameterModel());

            return Ok(relationships.Select(y => y.CollectionItemRelationshipName));
        }
    }
}
