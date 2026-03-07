using LinqKit;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sammlerplattform.Data;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses
{
    public interface IProcessConcept
    {
        (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        List<ConceptualRelationshipOperationParameterModel> Get(ConceptualRelationshipSearchParameterModel conceptSearchParameter);
        (int Statuscode, string StatusMessage) Delete(int conceptID);
    }

    public class ConceptualRelationshipProcessor(
        IUnitOfWork unitOfWork
        , DbIdentityContext context
        , IProcessConceptRelation processConceptRelation
        , IDeeplTranslationService translationService
        , IProcessTranslations processTranslations
        , ITranslationStore translationStore
        , ITrackEventsCSV trackEvents) : IProcessConcept
    {
        public (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (string.IsNullOrWhiteSpace(conceptualRelationshipOperation.ConceptViewModel.Name))
            {
                trackEvents.TrackError("ConceptualRelationshipProcessor/Insert: Concept Name is missing.", new Dictionary<string, object>
                {
                    {"Concept", conceptualRelationshipOperation.ConceptViewModel }
                });
                return (conceptualRelationshipOperation.ConceptViewModel.GetRootConceptId(), conceptualRelationshipOperation.ConceptViewModel.CollectionAreaID, 400, "Error_Concept_NameMissing");
            }

            ConceptualRelationshipSearchParameterModel searchParameterModel = new()
            { 
                Id = [.. unitOfWork.EntityTranslationRepository.Get(et =>
                        et.EntityType == nameof(Concept) &&
                        et.FieldName == nameof(ConceptViewModel.Name) &&
                        et.TranslatedText == conceptualRelationshipOperation.ConceptViewModel.Name)
                        .Select(et => et.EntityId).Distinct()]
            };
            if (searchParameterModel.Id.Count > 0)
            {
                ConceptViewModel? existingConcept = Get(searchParameterModel).Select(x => x.ConceptViewModel).FirstOrDefault();
                if (existingConcept != null)
                {
                    trackEvents.TrackError("ConceptualRelationshipProcessor/Insert: Concept already exists.", new Dictionary<string, object>
                    {
                        {"Concept", conceptualRelationshipOperation.ConceptViewModel },
                        {"ExistingConcept", existingConcept }
                    });
                    return (existingConcept.GetRootConceptId(), null, 409, "Error_Concept_Exists");
                }
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                Concept concept = new()
                {
                    CollectionAreaID = conceptualRelationshipOperation.ConceptViewModel.CollectionAreaID,
                    ConceptTypeInt = conceptualRelationshipOperation.ConceptViewModel.ConceptTypeInt ?? 0,
                    RootConceptID = conceptualRelationshipOperation.ConceptViewModel.RootConceptID
                };
                Concept newConcept = unitOfWork.ConceptRepository.Insert(concept);
                unitOfWork.Save();

                processTranslations.Insert(
                    new EntityTranslation
                    {
                        EntityType = nameof(Concept),
                        FieldName = nameof(ConceptViewModel.Name),
                        EntityId = newConcept.Id,
                        TranslatedText = conceptualRelationshipOperation.ConceptViewModel.Name,
                        Abbreviation = conceptualRelationshipOperation.ConceptViewModel.Abbreviation,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    },
                    conceptualRelationshipOperation.ConceptViewModel.Name);

                foreach (ConceptRelationViewModel relation in conceptualRelationshipOperation.ConceptRelationList)
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
                    { "Concept", conceptualRelationshipOperation.ConceptViewModel }
                });
                return (conceptualRelationshipOperation.ConceptViewModel.GetRootConceptId(), null, 500, "Error_Error_Ocurred");
            }
        }

        public (int Statuscode, string StatusMessage) Delete(int conceptID)
        {
            ConceptViewModel? existingConcept = Get(new ConceptualRelationshipSearchParameterModel { Id = [conceptID] }).FirstOrDefault()?.ConceptViewModel;
            if (existingConcept == null)
            {
                trackEvents.TrackError("ConceptualRelationshipProcessor/DeleteConcept: Concept not found.", new Dictionary<string, object>
                {
                    {"ConceptID", conceptID }
                });
                return (404, "Error_Concept_NotFound");
            }
            if (existingConcept.RootConceptID != null)
            {
                trackEvents.TrackError("ConceptualRelationshipProcessor/DeleteConcept: Concept is not a root concept.", new Dictionary<string, object>
                {
                    {"ConceptID", conceptID },
                    {"RootConceptID", existingConcept.RootConceptID }
                });
                return (400, "Error_Only_Root_Concept_Can_Be_Deleted");
            }
            if(existingConcept.CollectionAreaID != null)
            {
                trackEvents.TrackError("ConceptualRelationshipProcessor/DeleteConcept: Concept is assigned to a collection area.", new Dictionary<string, object>
                {
                    {"ConceptID", conceptID },
                    {"CollectionAreaID", existingConcept.CollectionAreaID }
                });
                return (400, "Error_Concept_Assigned_To_CollectionArea");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                List<ConceptRelationViewModel> existingRelations = processConceptRelation.GetByRootConceptID(existingConcept.Id);
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

        public List<ConceptualRelationshipOperationParameterModel> Get(ConceptualRelationshipSearchParameterModel conceptSearchParameter)
        {
            var predicate = PredicateBuilder.New<ConceptViewModel>(true);
            List<ConceptViewModel> cvmList = [.. context.Concept
                            .AsGraphNode(c => new Concept
                            {
                                Id = c.Id,
                                CollectionAreaID = c.CollectionAreaID,
                                ConceptTypeInt = c.ConceptTypeInt,
                                RootConceptID = c.RootConceptID
                            })
                            .Select(x => new ConceptViewModel
                            {
                                Id = x.Id,
                                CollectionAreaID = x.CollectionAreaID,
                                ConceptTypeInt = x.ConceptTypeInt,
                                RootConceptID = x.RootConceptID
                            })];
            
            if (conceptSearchParameter.ConceptTypeInt != null && conceptSearchParameter.ConceptTypeInt.Count > 0)
            {
                predicate = predicate.And(c => c.ConceptTypeInt.Equals(conceptSearchParameter.ConceptTypeInt[0]));
            }
            if (conceptSearchParameter.Id != null && conceptSearchParameter.Id.Count > 0)
            {
                foreach (int id in conceptSearchParameter.Id)
                {
                    predicate = predicate.And(c => c.Id.Equals(id));
                    if (conceptSearchParameter.RootConceptID != null && conceptSearchParameter.RootConceptID.Count > 0)
                    {
                        foreach (int? rootConceptId in conceptSearchParameter.RootConceptID)
                        {
                            predicate = predicate.Or(c => c.RootConceptID.Equals(rootConceptId));
                        }
                    }
                }
            }
            else if (conceptSearchParameter.RootConceptID != null && conceptSearchParameter.RootConceptID.Count > 0)
            {
                foreach (int? rootConceptId in conceptSearchParameter.RootConceptID)
                {
                    predicate = predicate.And(c => c.RootConceptID.Equals(rootConceptId));
                }
            }
            if (conceptSearchParameter.CollectionAreaID.Count > 0)
            {
                foreach (int? collectionAreaId in conceptSearchParameter.CollectionAreaID)
                {
                    predicate = predicate.And(c => c.CollectionAreaID.Equals(collectionAreaId));
                }
                predicate = predicate.Or(c => c.CollectionAreaID == null);
            }
            if (predicate.IsStarted)
            {
                cvmList = [.. cvmList.Where(predicate)];
            }

            foreach (ConceptViewModel concept in cvmList)
            {
                concept.Name = processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
                                {
                                    EntityType = [nameof(Concept)],
                                    EntityId = [concept.Id],
                                    FieldName = [nameof(ConceptViewModel.Name)],
                                    Culture = [translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)]
                                }).Select(x => x.TranslatedText).FirstOrDefault() ?? string.Empty;
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

            return [.. cvmList
                .OrderBy(x => x.Name)
                .Select(c => new ConceptualRelationshipOperationParameterModel
                {
                    ConceptViewModel = c
                })];
        }

        public (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (conceptualRelationshipOperation.ConceptViewModel.Id == 0)
            {
                return (conceptualRelationshipOperation.ConceptViewModel.GetRootConceptId(), conceptualRelationshipOperation.ConceptViewModel.CollectionAreaID, 400, "Error_Concept_IDMissing");
            }
            if (string.IsNullOrWhiteSpace(conceptualRelationshipOperation.ConceptViewModel.Name))
            {
                return (conceptualRelationshipOperation.ConceptViewModel.GetRootConceptId(), conceptualRelationshipOperation.ConceptViewModel.CollectionAreaID, 400, "Error_Concept_NameMissing");
            }

            Concept? existingConcept = unitOfWork.ConceptRepository.Get(c =>
                c.Id == conceptualRelationshipOperation.ConceptViewModel.Id).FirstOrDefault();
            if (existingConcept == null)
            {
                return (conceptualRelationshipOperation.ConceptViewModel.GetRootConceptId(), conceptualRelationshipOperation.ConceptViewModel.CollectionAreaID, 404, "Error_Concept_NotFound");
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
                if (existingTranslations.First(x => x.FieldName == nameof(ConceptViewModel.Name)).TranslatedText != conceptualRelationshipOperation.ConceptViewModel.Name)
                {
                    processTranslations.Update(
                        new EntityTranslation
                        {
                            EntityType = nameof(Concept),
                            EntityId = existingConcept.Id,
                            FieldName = nameof(ConceptViewModel.Name),
                            TranslatedText = conceptualRelationshipOperation.ConceptViewModel.Name,
                            Abbreviation = conceptualRelationshipOperation.ConceptViewModel.Abbreviation,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        conceptualRelationshipOperation.ConceptViewModel.Name);
                }
                if (!string.IsNullOrEmpty(conceptualRelationshipOperation.ConceptViewModel.Description))
                {
                    if (existingTranslations.FirstOrDefault(x => x.FieldName == nameof(ConceptViewModel.Description))?.TranslatedText != conceptualRelationshipOperation.ConceptViewModel.Description)
                    {
                        processTranslations.Update(
                            new EntityTranslation
                            {
                                EntityType = nameof(Concept),
                                EntityId = existingConcept.Id,
                                FieldName = nameof(ConceptViewModel.Description),
                                TranslatedText = conceptualRelationshipOperation.ConceptViewModel.Description,
                                Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                            },
                            conceptualRelationshipOperation.ConceptViewModel.Description);
                    }
                    else
                    {
                        processTranslations.Insert(
                            new EntityTranslation
                            {
                                EntityType = nameof(Concept),
                                EntityId = existingConcept.Id,
                                FieldName = nameof(ConceptViewModel.Description),
                                TranslatedText = conceptualRelationshipOperation.ConceptViewModel.Description,
                                Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                            },
                            conceptualRelationshipOperation.ConceptViewModel.Description);
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
                    { "Concept", conceptualRelationshipOperation.ConceptViewModel }
                });
                return (existingConcept.GetRootConceptId(), null, 500, "Error_Error_Ocurred");
            }
        }

        private void ConnectRelationToConcept(Concept newConcept, ConceptRelationViewModel relation)
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
        private void SyncRelationConnections(Concept existingConcept, List<ConceptRelationViewModel> newConnections)
        {
            List<ConceptRelationViewModel> currentConnections = processConceptRelation.GetByRootConceptID(existingConcept.Id);

            foreach (ConceptRelationViewModel current in currentConnections)
            {
                ConceptRelationViewModel? updated = newConnections.FirstOrDefault(x => x.ToConceptID == x.ToConceptID);

                if (updated == null)
                {
                    _ = processConceptRelation.Delete(current);
                }
                else if (updated.RelationTypeInt != current.RelationTypeInt || updated.IsDirected != current.IsDirected)
                {
                    _ = processConceptRelation.Update(updated);
                }
            }

            foreach (ConceptRelationViewModel newItem in newConnections)
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
