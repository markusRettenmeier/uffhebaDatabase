using Sammlerplattform.Data;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses
{
    public interface IProcessConcept
    {
        (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        List<ConceptualRelationshipOperationParameterModel> GetWithPredicates(ConceptualRelationshipSearchParameterModel conceptSearchParameter);
        (int Statuscode, string StatusMessage) Delete(int conceptID);
    }

    public class ConceptualRelationshipProcessor(IUnitOfWork unitOfWork
        , IProcessConceptRelation processConceptRelation
        , IDeeplTranslationService translationService
        , IProcessTranslations processTranslations
        , ITranslationStore translationStore
        , ITrackEvents trackEvents) : IProcessConcept
    {
        public (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (string.IsNullOrWhiteSpace(conceptualRelationshipOperation.Concept.Name))
            {
                trackEvents.TrackWarning("ConceptualRelationshipProcessor/Insert: Concept Name is missing.", new Dictionary<string, object>
                {
                    {"Concept", conceptualRelationshipOperation.Concept }
                });
                return (conceptualRelationshipOperation.Concept.GetRootConceptId(), conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Error_Concept_NameMissing");
            }

            ConceptualRelationshipSearchParameterModel searchParameterModel = new()
            { 
                Id = [.. unitOfWork.EntityTranslationRepository.Get(et =>
                        et.EntityType == nameof(Concept) &&
                        et.FieldName == nameof(Concept.Name) &&
                        et.TranslatedText == conceptualRelationshipOperation.Concept.Name)
                        .Select(et => et.EntityId).Distinct()]
            };
            if(searchParameterModel.Id.Count == 0)
            {
                searchParameterModel.Id = [0]; // To prevent querying all concepts when no matching translations are found
            }
            Concept? existingConcept = GetWithPredicates(searchParameterModel).Select(x => x.Concept).FirstOrDefault();
            if (existingConcept != null)
            {
                trackEvents.TrackWarning("ConceptualRelationshipProcessor/Insert: Concept already exists.", new Dictionary<string, object>
                {
                    {"Concept", conceptualRelationshipOperation.Concept },
                    {"ExistingConcept", existingConcept }
                });
                return (existingConcept.GetRootConceptId(), null, 409, "Error_Concept_Exists");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                Concept newConcept = unitOfWork.ConceptRepository.Insert(conceptualRelationshipOperation.Concept);
                unitOfWork.Save();

                processTranslations.Insert(
                    new EntityTranslation
                    {
                        EntityType = nameof(Concept),
                        FieldName = nameof(Concept.Name),
                        EntityId = newConcept.Id,
                        TranslatedText = conceptualRelationshipOperation.Concept.Name,
                        Abbreviation = conceptualRelationshipOperation.Concept.Abbreviation,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    conceptualRelationshipOperation.Concept.Name);

                foreach (ConceptRelation relation in conceptualRelationshipOperation.ConceptRelationList)
                {
                    ConnectRelationToConcept(newConcept, relation);
                }

                transactionScope.Complete();
                return (newConcept.GetRootConceptId(), null, 201, "Success_Concept_Created");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ConceptualRelationshipProcessor/Insert", new Dictionary<string, object>
                {
                    { "Concept", conceptualRelationshipOperation.Concept }
                });
                return (conceptualRelationshipOperation.Concept.GetRootConceptId(), null, 500, "Error_Error_Ocurred");
            }
        }

        public (int Statuscode, string StatusMessage) Delete(int conceptID)
        {
            Concept? existingConcept = GetWithPredicates(new ConceptualRelationshipSearchParameterModel { Id = [conceptID] }).FirstOrDefault()?.Concept;
            if (existingConcept == null)
            {
                trackEvents.TrackWarning("ConceptualRelationshipProcessor/DeleteConcept: Concept not found.", new Dictionary<string, object>
                {
                    {"ConceptID", conceptID }
                });
                return (404, "Error_Concept_NotFound");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                List<ConceptRelation> existingRelations = processConceptRelation.GetByConceptID(existingConcept.Id);
                for (int i = 0; i < existingRelations.Count; i++)
                {
                    _ = processConceptRelation.Delete(existingRelations[i]);
                }

                unitOfWork.ConceptRepository.Delete(existingConcept);
                unitOfWork.Save();

                transactionScope.Complete();
                return (200, "Success_Concept_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ConceptualRelationshipProcessor/DeleteConcept", new Dictionary<string, object>
                {
                    { "ConceptID", conceptID }
                });
                return (500, "Error_Error_Ocurred");
            }
        }

        public List<ConceptualRelationshipOperationParameterModel> GetWithPredicates(ConceptualRelationshipSearchParameterModel conceptSearchParameter)
        {
            IEnumerable<Concept> query = unitOfWork.ConceptRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Concept>(conceptSearchParameter),
                includeProperties: nameof(Concept.CollectionArea) + "," +
                                   nameof(Concept.CollectionItemEntityList));

            foreach (Concept concept in query)
            {
                concept.Name = translationStore.GetTranslation(
                    nameof(Concept),
                    concept.Id,
                    nameof(concept.Name),
                    concept.Name)
                    ?? concept.Name;
                if (!string.IsNullOrEmpty(concept.Description))
                {
                    concept.Description = translationStore.GetTranslation(
                        nameof(Concept),
                        concept.Id,
                        nameof(concept.Description),
                        concept.Description)
                        ?? concept.Description;
                }
            }

            return [.. query
                .OrderBy(x => x.Name)
                .Select(c => new ConceptualRelationshipOperationParameterModel
                {
                    Concept = c,
                    ConceptRelationList = processConceptRelation.GetByConceptID(c.Id)
                })];
        }

        public (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (conceptualRelationshipOperation.Concept.Id == 0)
            {
                return (conceptualRelationshipOperation.Concept.GetRootConceptId(), conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Error_Concept_IDMissing");
            }
            if (string.IsNullOrWhiteSpace(conceptualRelationshipOperation.Concept.Name))
            {
                return (conceptualRelationshipOperation.Concept.GetRootConceptId(), conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Error_Concept_NameMissing");
            }

            Concept? existingConcept = unitOfWork.ConceptRepository.Get(c =>
                c.Id == conceptualRelationshipOperation.Concept.Id).FirstOrDefault();
            if (existingConcept == null)
            {
                return (conceptualRelationshipOperation.Concept.GetRootConceptId(), conceptualRelationshipOperation.Concept.CollectionAreaID, 404, "Error_Concept_NotFound");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                var existingTranslations = processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(Concept)],
                    EntityId = [existingConcept.Id],
                    Culture = [translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)]
                });
                if (existingTranslations.First(x => x.FieldName == nameof(Concept.Name)).TranslatedText != conceptualRelationshipOperation.Concept.Name)
                {
                    //existingConcept.Name = translationService.SetIntoFallbackLanguage(conceptualRelationshipOperation.Concept.Name);
                    processTranslations.Update(
                        new EntityTranslation
                        {
                            EntityType = nameof(Concept),
                            EntityId = existingConcept.Id,
                            FieldName = nameof(existingConcept.Name),
                            TranslatedText = conceptualRelationshipOperation.Concept.Name,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        conceptualRelationshipOperation.Concept.Name);
                }
                if (!string.IsNullOrEmpty(conceptualRelationshipOperation.Concept.Description))
                {
                    if (existingTranslations.FirstOrDefault(x => x.FieldName == nameof(Concept.Description))?.TranslatedText != conceptualRelationshipOperation.Concept.Description)
                    {
                        processTranslations.Update(
                            new EntityTranslation
                            {
                                EntityType = nameof(Concept),
                                EntityId = existingConcept.Id,
                                FieldName = nameof(existingConcept.Description),
                                TranslatedText = conceptualRelationshipOperation.Concept.Description,
                                Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                            },
                            conceptualRelationshipOperation.Concept.Description);
                    }
                    else
                    {
                        processTranslations.Insert(
                            new EntityTranslation
                            {
                                EntityType = nameof(Concept),
                                EntityId = existingConcept.Id,
                                FieldName = nameof(Concept.Description),
                                TranslatedText = conceptualRelationshipOperation.Concept.Description,
                                Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                            },
                            conceptualRelationshipOperation.Concept.Description);
                    }
                }

                SyncRelationConnections(existingConcept, conceptualRelationshipOperation.ConceptRelationList);

                transactionScope.Complete();
                return (existingConcept.GetRootConceptId(), null, 201, "Success_Concept_Updated");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ConceptualRelationshipProcessor/Update", new Dictionary<string, object>
                {
                    { "Concept", conceptualRelationshipOperation.Concept }
                });
                return (existingConcept.GetRootConceptId(), null, 500, "Error_Error_Ocurred");
            }
        }

        private void ConnectRelationToConcept(Concept newConcept, ConceptRelation relation)
        {
            if (relation.ToConceptID == 0)
            {
                return;
            }

            relation.FromConceptID = newConcept.Id; // Ensure FromConceptID is set to the newly created concept
            if (relation.FromConceptID == relation.ToConceptID)
            {
                return;
            }

            if (relation.RelationTypeInt is 0 or 2)
            {
                relation.IsDirected = false;
            }

            _ = processConceptRelation.Insert(relation);
        }
        private void SyncRelationConnections(Concept existingConcept, List<ConceptRelation> newConnections)
        {
            List<ConceptRelation> currentConnections = processConceptRelation.GetByConceptID(existingConcept.Id);

            foreach (ConceptRelation current in currentConnections)
            {
                ConceptRelation? updated = newConnections.FirstOrDefault(x => x.ToConceptID == x.ToConceptID);

                if (updated == null)
                {
                    _ = processConceptRelation.Delete(current);
                }
                else if (updated.RelationTypeInt != current.RelationTypeInt || updated.IsDirected != current.IsDirected)
                {
                    _ = processConceptRelation.Update(updated);
                }
            }

            foreach (ConceptRelation newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ToConceptID == newItem.ToConceptID);
                if (!exists)
                {
                    ConnectRelationToConcept(existingConcept, newItem);
                }
            }
        }
    }
}
