using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.Extensions;
using System.Data.Entity;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses
{
    public interface IProcessCollectionArea
    {
        List<CollectionArea> GetEntityListViaPredicate(CollectionAreaSearchParameterModel searchParameterModel);
        List<CollectionAreaDisplayDTO> GetWithTranslationsListViaPredicate(CollectionAreaSearchParameterModel searchParameterModel);
        (int StatusCode, string StatusMessage, int CollectionAreaID) Insert(CollectionAreaCreateDTO createDTO);
        (int StatusCode, string StatusMessage, int CollectionAreaID) Update(CollectionAreaEditDTO editDTO);
        (int StatusCode, string StatusMessage) Delete(int id);
    }
    public class CollectionAreaProcessor(IUnitOfWork unitOfWork,
        IProcessTranslations processTranslations,
        ITrackEventsText trackEvents,
        IProcessConcept processConcept) : IProcessCollectionArea
    {
        public (int StatusCode, string StatusMessage, int CollectionAreaID) Insert(CollectionAreaCreateDTO createDTO)
        {
            int collectionAreaID = CheckIfNameExists(createDTO.Name);
            if (collectionAreaID > 0)
            {
                return (409, "Error_CollectionArea_Exists", collectionAreaID);
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionArea newCollection = new()
                {
                    WikipediaUrl = createDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                };
                newCollection = unitOfWork.CollectionAreaRepository.Insert(newCollection);
                unitOfWork.Save();

                List<string> translationList = processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Name,
                        EntityName = nameof(CollectionArea),
                        EntityId = newCollection.CollectionAreaID,
                        PropertyName = nameof(CollectionAreaDisplayDTO.CollectionAreaName)
                    });

                processConcept.Insert(new ConceptCreateDTO
                {
                    Name = createDTO.Name,
                    CollectionAreaID = newCollection.CollectionAreaID
                });

                transactionScope.Complete();
                return (201, "Success_CollectionArea_Created", newCollection.CollectionAreaID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Creating CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionAreaName", createDTO.Name }
                });
                return (500, "Error_Unknown", 0);
            }
        }

        public (int StatusCode, string StatusMessage, int CollectionAreaID) Update(CollectionAreaEditDTO editDTO)
        {
            CollectionAreaSearchParameterModel collectionSearchParameterModel = new() { CollectionAreaID = [editDTO.Id] };
            CollectionArea? existingCollection = GetEntityListViaPredicate(collectionSearchParameterModel).FirstOrDefault();
            if (existingCollection == null)
            {
                trackEvents.TrackError("Attempted to edit non-existing CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionArea", editDTO }
                });
                return (404, "Error_CollectionArea_NotFound", 0);
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                string? editedWikipediaUrlWithoutSubdomain = editDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain();
                if (editDTO.WikipediaUrl != editedWikipediaUrlWithoutSubdomain)
                {
                    existingCollection.WikipediaUrl = editedWikipediaUrlWithoutSubdomain;
                    unitOfWork.Save();
                }

                processTranslations.Update(
                    new TranslationDTO
                    {
                        TextToTranslate = editDTO.Name,
                        EntityName = nameof(CollectionArea),
                        EntityId = existingCollection.CollectionAreaID,
                        PropertyName = nameof(CollectionAreaDisplayDTO.CollectionAreaName)
                    });

                List<EntityTranslation> conceptTranslationList = [.. processTranslations.GetWithFallback(new EntityTranslationSearchParameter
                    {
                        EntityName = [nameof(Concept)],
                        PropertyName = [nameof(ConceptViewModel.Name)],
                        TranslatedText = [editDTO.Name]
                    })];
                processConcept.Update(new ConceptEditDTO
                {
                    Id = conceptTranslationList.Select(et => et.EntityId).FirstOrDefault(), // Assuming there is only one linked concept, otherwise this needs to be adapted
                    Name = editDTO.Name,
                    CollectionAreaID = existingCollection.CollectionAreaID
                });

                transactionScope.Complete();
                return (200, "Success_CollectionArea_Updated", existingCollection.CollectionAreaID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Editing CollectionArea", new Dictionary<string, object>
                {
                    {"CollectionArea", editDTO }
                });
                return (500, "Error_Unknown", 0);
            }
        }

        public List<CollectionAreaDisplayDTO> GetWithTranslationsListViaPredicate(CollectionAreaSearchParameterModel searchParameterModel)
        {
            foreach (string name in searchParameterModel.CollectionAreaName)
            {
                int collectionAreaID = CheckIfNameExists(name);
                if (collectionAreaID > 0)
                {
                    searchParameterModel.CollectionAreaID.Add(collectionAreaID);
                }
            }
            searchParameterModel.CollectionAreaName = [];

            var collectionAreaList = unitOfWork.CollectionAreaRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionArea>(searchParameterModel),
                includeProperties: nameof(CollectionArea.StatePreservationList) + "," + nameof(CollectionArea.ConceptList))
                .AsNoTracking()
                .Select(ca => new CollectionAreaDisplayDTO
                {
                    Id = ca.CollectionAreaID,
                    WikipediaUrl = ca.WikipediaUrl,
                    StatePreservationList = ca.StatePreservationList,
                    ConceptList = ca.ConceptList
                }).ToList();
            SetTranslations(collectionAreaList);

            return [.. collectionAreaList.OrderBy(x => x.CollectionAreaName)];
        }

        private void SetTranslations(List<CollectionAreaDisplayDTO> collectionAreaList)
        {
            var allCollectionAreaTranslations = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(CollectionArea)],
                PropertyName = [nameof(CollectionAreaDisplayDTO.CollectionAreaName)]
            });
            foreach (CollectionAreaDisplayDTO collectionArea in collectionAreaList)
            {
                collectionArea.CollectionAreaName= allCollectionAreaTranslations
                    .FirstOrDefault(t => t.EntityId == collectionArea.Id)?.TranslatedText ?? string.Empty;
            }
        }

        private int CheckIfNameExists(string name)
        {
            int? collectionAreaId = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(CollectionArea)],
                PropertyName = [nameof(CollectionAreaDisplayDTO.CollectionAreaName)],
                TranslatedText = [name]
            }).Select(et => et.EntityId).FirstOrDefault();

            return collectionAreaId ?? 0;
        }

        public (int StatusCode, string StatusMessage) Delete(int id)
        {
            CollectionArea? collectionArea = GetEntityListViaPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = [id] }).FirstOrDefault();
            if (collectionArea == null)
            {
                trackEvents.TrackError("Attempted to delete non-existing CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id }
                });
                return (404, "Error_CollectionArea_NotFound");
            }
            if (collectionArea.ConceptList.Count != 0)
            {
                trackEvents.TrackError("Attempted to delete CollectionArea with linked Concepts", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id },
                    { "LinkedConceptIDs", string.Join(", ", collectionArea.ConceptList.Select(c => c.Id)) }
                });
                return (409, "Error_CollectionArea_LinkedConcepts");
            }
            if (collectionArea.StatePreservationList.Count != 0)
            {
                trackEvents.TrackError("Attempted to delete CollectionArea with linked StatePreservations", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id },
                    { "LinkedStatePreservationIDs", string.Join(", ", collectionArea.StatePreservationList.Select(s => s.StatePreservationID)) }
                });
                return (409, "Error_CollectionArea_LinkedStatePreservations");
            }
            if (collectionArea.CollectionItemEntityList.Count != 0)
            {
                trackEvents.TrackError("Attempted to delete CollectionArea with linked CollectionItems", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id },
                    { "LinkedCollectionItemIDs", string.Join(", ", collectionArea.CollectionItemEntityList.Select(cie => cie.CollectionItemEntityID)) }
                });
                return (409, "Error_CollectionArea_LinkedCollectionItems");
            }

            try
            {
                using TransactionScope transactionScope = new();

                processConcept.Delete(collectionArea.ConceptList.First().Id); // Assuming there is only one linked concept (named after the collectionAreaName), otherwise this needs to be adapted

                processTranslations.Delete(new EntityTranslationSearchParameter
                {
                    EntityName = [nameof(CollectionArea)],
                    EntityId = [collectionArea.CollectionAreaID],
                    PropertyName = [nameof(CollectionAreaDisplayDTO.CollectionAreaName)]
                });

                unitOfWork.CollectionAreaRepository.Delete(collectionArea);
                unitOfWork.Save();

                transactionScope.Complete();
                return (200, "Success_CollectionArea_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Deleting CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id }
                });
                return (500, "Error_Unknown");
            }
        }

        public List<CollectionArea> GetEntityListViaPredicate(CollectionAreaSearchParameterModel searchParameterModel)
        {
            IQueryable<CollectionArea> query = unitOfWork.CollectionAreaRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionArea>(searchParameterModel),
                includeProperties: nameof(CollectionArea.StatePreservationList) + "," + nameof(CollectionArea.ConceptList) + "," + nameof(CollectionArea.CollectionItemEntityList));
            return [.. query];
        }
    }
}