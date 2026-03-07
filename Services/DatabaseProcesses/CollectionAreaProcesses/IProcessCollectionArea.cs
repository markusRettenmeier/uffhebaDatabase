using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses
{
    public interface IProcessCollectionArea
    {
        List<CollectionArea> GetListWithPredicate(CollectionAreaSearchParameterModel searchParameterModel);
        (int StatusCode, string StatusMessage, int CollectionAreaID) Insert(CollectionArea collectionArea);
        (int StatusCode, string StatusMessage, int CollectionAreaID) Update(CollectionArea collectionArea);
        (int StatusCode, string StatusMessage) Delete(int id);
    }
    public class CollectionAreaProcessor(IUnitOfWork unitOfWork,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEventsCSV trackEvents,
        IProcessConcept processConcept) : IProcessCollectionArea
    {
        public (int StatusCode, string StatusMessage, int CollectionAreaID) Insert(CollectionArea collectionArea)
        {
            CollectionAreaSearchParameterModel searchParameterModel = new()
            {
                CollectionAreaID = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(CollectionArea)],
                        FieldName = [nameof(CollectionArea.CollectionAreaName)],
                        TranslatedText = [collectionArea.CollectionAreaName]
                    }).Select(et => et.EntityId).Distinct()]
            };
            if (searchParameterModel.CollectionAreaID.Count > 0)
            {
                trackEvents.TrackError("Attempted to create duplicate CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionAreaName", collectionArea.CollectionAreaName }
                });
                return (409, "Error_CollectionArea_Exists", 0);
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                CollectionArea newCollection = unitOfWork.CollectionAreaRepository.Insert(collectionArea);
                unitOfWork.Save();

                processTranslations.Insert(
                    new EntityTranslation
                    {
                        EntityType = nameof(CollectionArea),
                        EntityId = newCollection.CollectionAreaID,
                        FieldName = nameof(newCollection.CollectionAreaName),
                        TranslatedText = collectionArea.CollectionAreaName,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    collectionArea.CollectionAreaName);

                ConceptViewModel newConcept = new()
                {
                    Name = collectionArea.CollectionAreaName,
                    CollectionAreaID = newCollection.CollectionAreaID
                };
                processConcept.Insert(new ConceptualRelationshipOperationParameterModel
                {
                    ConceptViewModel = newConcept
                });

                transactionScope.Complete();
                return (201, "Success_CollectionArea_Created", newCollection.CollectionAreaID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Creating CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionAreaName", collectionArea.CollectionAreaName }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int StatusCode, string StatusMessage, int CollectionAreaID) Update(CollectionArea collectionArea)
        {
            if (collectionArea.CollectionAreaID <= 0)
            {
                trackEvents.TrackError("Attempted to edit CollectionArea with missing ID", new Dictionary<string, object> {
                    {
                        "CollectionArea", collectionArea
                    }
                });
                return (400, "Error_CollectionArea_IdMissing", 0);
            }

            CollectionAreaSearchParameterModel collectionSearchParameterModel = new() { CollectionAreaID = [collectionArea.CollectionAreaID] };
            CollectionArea? existingCollection = GetListWithPredicate(collectionSearchParameterModel).FirstOrDefault();
            if (existingCollection == null)
            {
                trackEvents.TrackError("Attempted to edit non-existing CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionArea", collectionArea }
                });
                return (404, "Error_CollectionArea_NotFound", 0);
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (collectionArea.WikipediaUrl != existingCollection.WikipediaUrl)
                {
                    existingCollection.WikipediaUrl = collectionArea.WikipediaUrl;
                    unitOfWork.Save();
                }

                var existingTranslations = processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(CollectionArea)],
                    EntityId = [collectionArea.CollectionAreaID],
                    FieldName = [nameof(CollectionArea.CollectionAreaName)],
                    Culture = [translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)]
                }).FirstOrDefault();
                if (existingTranslations != null && existingTranslations.TranslatedText != collectionArea.CollectionAreaName)
                {
                    processTranslations.Update(
                        new EntityTranslation
                        {
                            EntityType = nameof(CollectionArea),
                            EntityId = collectionArea.CollectionAreaID,
                            FieldName = nameof(CollectionArea.CollectionAreaName),
                            TranslatedText = collectionArea.CollectionAreaName,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        collectionArea.CollectionAreaName);
                }

                Concept? linkedConcept = unitOfWork.ConceptRepository.Get(
                    filter: c => c.CollectionAreaID == existingCollection.CollectionAreaID).FirstOrDefault();
                if (linkedConcept != null)
                {
                    var conceptTranslations = processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(Concept)],
                        EntityId = [linkedConcept.Id],
                        FieldName = [nameof(ConceptViewModel.Name)],
                        Culture = [translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)]
                    }).FirstOrDefault();
                    if (conceptTranslations != null && conceptTranslations.TranslatedText != collectionArea.CollectionAreaName)
                    {
                        processTranslations.Update(
                            new EntityTranslation
                            {
                                EntityType = nameof(Concept),
                                EntityId = linkedConcept.Id,
                                FieldName = nameof(ConceptViewModel.Name),
                                TranslatedText = collectionArea.CollectionAreaName,
                                Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                            },
                            collectionArea.CollectionAreaName);
                    }
                }

                transactionScope.Complete();
                return (200, "Success_CollectionArea_Updated", existingCollection.CollectionAreaID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Editing CollectionArea", new Dictionary<string, object>
                {
                    {"CollectionArea", collectionArea }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public List<CollectionArea> GetListWithPredicate(CollectionAreaSearchParameterModel searchParameterModel)
        {
            IEnumerable<CollectionArea> query = unitOfWork.CollectionAreaRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionArea>(searchParameterModel),
                includeProperties: nameof(CollectionArea.StatePreservationList) + "," + nameof(CollectionArea.ConceptList));

            foreach (CollectionArea collectionArea in query)
            {
                collectionArea.CollectionAreaName = translationStore.GetTranslation(
                        nameof(CollectionArea),
                        collectionArea.CollectionAreaID,
                        nameof(collectionArea.CollectionAreaName),
                        translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? collectionArea.CollectionAreaName;
            }

            return [.. query.OrderBy(x => x.CollectionAreaName)];
        }

        public (int StatusCode, string StatusMessage) Delete(int id)
        {
            CollectionArea? collectionArea = GetListWithPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = [id] }).FirstOrDefault();
            if (collectionArea == null)
            {
                trackEvents.TrackError("Attempted to delete non-existing CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id }
                });
                return (404, "Error_CollectionArea_NotFound");
            }

            if (collectionArea.ConceptList.Count > 0)
            {
                trackEvents.TrackError("Attempted to delete CollectionArea with linked Concepts", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id },
                    { "LinkedConceptIDs", string.Join(", ", collectionArea.ConceptList.Select(c => c.Id)) }
                });
                return (400, "Error_CollectionArea_HasLinkedConcepts");
            }
            if (collectionArea.StatePreservationList.Count > 0)
            {
                trackEvents.TrackError("Attempted to delete CollectionArea with linked StatePreservations", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id },
                    { "LinkedStatePreservationIDs", string.Join(", ", collectionArea.StatePreservationList.Select(sp => sp.StatePreservationID)) }
                });
                return (400, "Error_CollectionArea_HasLinkedStatePreservations");
            }
            if (collectionArea.CollectionItemEntityList.Count > 0)
            {
                trackEvents.TrackError("Attempted to delete CollectionArea with linked CollectionItems", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id },
                    { "LinkedCollectionItemIDs", string.Join(", ", collectionArea.CollectionItemEntityList.Select(cie => cie.CollectionItemEntityID)) }
                });
                return (400, "Error_CollectionArea_HasLinkedCollectionItems");
            }

            try
            {
                unitOfWork.CollectionAreaRepository.Delete(collectionArea);
                unitOfWork.Save();

                return (200, "Success_CollectionArea_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Deleting CollectionArea", new Dictionary<string, object>
                {
                    { "CollectionAreaID", id }
                });
                return (500, "Error_Error_Ocurred");
            }
        }
    }
}
