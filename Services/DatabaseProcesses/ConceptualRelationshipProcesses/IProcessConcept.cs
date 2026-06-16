using LinqKit;
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
        (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptCreateDTO createDto);
        (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptEditDTO editDto);
        List<ConceptDisplayDTO> Get(ConceptualRelationshipSearchParameterModel conceptSearchParameter);
        (int Statuscode, string StatusMessage) Delete(int conceptID);
    }

    public class ConceptualRelationshipProcessor(
        IUnitOfWork unitOfWork
        , DbIdentityContext context
        , IProcessConceptRelation processConceptRelation
        , IDeeplTranslationService translationService
        , IProcessTranslations processTranslations
        , ITrackEventsCSV trackEvents) : IProcessConcept
    {
        public (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptCreateDTO createDto)
        {
            ConceptualRelationshipSearchParameterModel searchParameterModel = new()
            {
                Id = [.. unitOfWork.EntityTranslationRepository.Get(et =>
                        et.EntityType == nameof(Concept) &&
                        et.FieldName == nameof(ConceptViewModel.Name) &&
                        et.TranslatedText == createDto.Name)
                        .Select(et => et.EntityId).Distinct()]
            };
            if (searchParameterModel.Id.Count > 0)
            {
                ConceptViewModel? existingConcept = Get(searchParameterModel).Select(x => x.ConceptViewModel).FirstOrDefault();
                if (existingConcept != null)
                {
                    trackEvents.TrackError("ConceptualRelationshipProcessor/Insert: Concept already exists.", new Dictionary<string, object>
                    {
                        {"Concept", createDto },
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
                    CollectionAreaID = createDto.CollectionAreaID,
                    ConceptTypeInt = createDto.ConceptTypeInt ?? 0,
                    RootConceptID = createDto.RootConceptID
                };
                Concept newConcept = unitOfWork.ConceptRepository.Insert(concept);
                unitOfWork.Save();

                processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDto.Name,
                        EntityType = nameof(Concept),
                        FieldName = nameof(ConceptViewModel.Name),
                        EntityId = newConcept.Id,
                        Abbreviation = createDto.Abbreviation,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name),
                        IsTranslateable = createDto.IsTranslateable
                    });

                foreach (var relation in createDto.ConceptRelationList)
                {
                    ConnectRelationToConcept(newConcept.Id, relation);
                }

                transactionScope.Complete();
                return (newConcept.GetRootConceptId(), null, 201, "Success_Concept_Created");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ConceptualRelationshipProcessor/Insert", new Dictionary<string, object>
                {
                    { "Concept", createDto }
                });
                return (createDto.RootConceptID ?? 0, null, 500, "Error_Unknown");
            }
        }

        public (int Statuscode, string StatusMessage) Delete(int conceptID)
        {
            ConceptViewModel? conceptViewModel = Get(new ConceptualRelationshipSearchParameterModel { Id = [conceptID] }).FirstOrDefault()?.ConceptViewModel;
            if (conceptViewModel == null)
            {
                trackEvents.TrackError("ConceptualRelationshipProcessor/DeleteConcept: Concept not found.", new Dictionary<string, object>
                {
                    {"ConceptID", conceptID }
                });
                return (404, "Error_Concept_NotFound");
            }
            if (conceptViewModel.CollectionAreaID != null && conceptViewModel.RootConceptID == null)
            {
                trackEvents.TrackError("ConceptualRelationshipProcessor/DeleteConcept: Concept is assigned to a collection area.", new Dictionary<string, object>
                {
                    {"ConceptID", conceptID },
                    {"CollectionAreaID", conceptViewModel.CollectionAreaID }
                });
                return (400, "Error_Concept_AssignedToCollectionArea");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                List<ConceptRelationViewModel> existingRelations = processConceptRelation.GetByRootConceptID(conceptViewModel.Id);
                for (int i = 0; i < existingRelations.Count; i++)
                {
                    _ = processConceptRelation.Delete(existingRelations[i]);
                }

                Concept existingConcept = new()
                {
                    Id = conceptViewModel.Id,
                    CollectionAreaID = conceptViewModel.CollectionAreaID,
                    ConceptTypeInt = (int)conceptViewModel.ConceptType,
                    RootConceptID = conceptViewModel.RootConceptID
                };
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
                return (500, "Error_Unknown");
            }
        }

        public List<ConceptDisplayDTO> Get(ConceptualRelationshipSearchParameterModel conceptSearchParameter)
        {
            List<string> conceptNameList = conceptSearchParameter.ConceptName ?? [];
            bool comingFromIndexGeneral = conceptSearchParameter.RootConceptID.Count == 1 && conceptSearchParameter.RootConceptID[0] == null; // In IndexGeneral, RootConceptID is explicitly set to null, in IndexSpecific it is not set at all (and thus also null, but the count is 0)

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
            if (conceptSearchParameter.ConceptName != null && conceptSearchParameter.ConceptName.Count > 0)
            {
                // ConceptName filter will be applied at the end, after the translations are applied, because otherwise the search term might not match due to missing translations
                conceptNameList = conceptSearchParameter.ConceptName;
            }
            if (predicate.IsStarted)
            {
                cvmList = [.. cvmList.Where(predicate)];
            }

            foreach (ConceptViewModel concept in cvmList)
            {
                EntityTranslation? translation = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(Concept)],
                    EntityId = [concept.Id],
                    FieldName = [nameof(ConceptViewModel.Name)],
                    Culture = [translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)]
                }).FirstOrDefault();
                if (translation != null)
                {
                    concept.Abbreviation = translation.Abbreviation;
                    concept.Name = translation.TranslatedText;
                }

                if (concept.RootConceptID == null)
                {
                    ConceptualRelationshipSearchParameterModel secondSearchParameters = new() { RootConceptID = [concept.Id] }; // Must be this way, cause otherwise conceptSearchParameter is reinitialted with the new parameters
                    var subConceptList = Get(secondSearchParameters);
                    concept.SubConceptNameList.AddRange(subConceptList.Select(x => x.ConceptViewModel.Name));
                }
                // Im Code dann:
                //if (concept.RootConceptID == null)
                //{
                //    var subConceptNames = GetSubConceptNames(concept.Id, conceptSearchParameter);
                //    concept.SubConceptNameList.AddRange(subConceptNames);
                //}
                //if (concept.RootConceptID == null)
                //{
                //    // Speichere die aktuellen ConceptName-Werte
                //    var originalConceptNames = conceptSearchParameter.ConceptName?.ToList();

                //    ConceptualRelationshipSearchParameterModel secondSearchParameters = new()
                //    {
                //        RootConceptID = [concept.Id]
                //    };

                //    // Optionale: Falls Sie andere Filter beibehalten wollen
                //    if (conceptSearchParameter.CollectionAreaID?.Count > 0)
                //        secondSearchParameters.CollectionAreaID = [.. conceptSearchParameter.CollectionAreaID];
                //    if (conceptSearchParameter.ConceptTypeInt?.Count > 0)
                //        secondSearchParameters.ConceptTypeInt = [.. conceptSearchParameter.ConceptTypeInt];

                //    var subConceptList = Get(secondSearchParameters);
                //    concept.SubConceptNameList.AddRange(subConceptList.Select(x => x.ConceptViewModel.Name));

                //    // Stelle die originalen ConceptName-Werte wieder her
                //    if (originalConceptNames != null)
                //        conceptSearchParameter.ConceptName = originalConceptNames;
                //}
            }

            //if(conceptSearchParameter.ConceptName.Count > 0)
            //{
            //    foreach (string conceptName in conceptSearchParameter.ConceptName)
            //    {
            //        cvmList = [.. cvmList.Where(c => (conceptSearchParameter.RootConceptID != null) 
            //        ? c.Name.Contains(conceptName, StringComparison.CurrentCultureIgnoreCase) // In IndexGeneral, search for concept name in all concepts
            //        : c.IsRootConcept || c.Name.Contains(conceptName, StringComparison.CurrentCultureIgnoreCase))]; // In IndexSpecific, also the rootconcept must be taken into account, even if the rootconcept name does not match the search term
            //    }
            //}

            if (conceptNameList.Count > 0)
            {
                foreach (string conceptName in conceptNameList)
                {
                    cvmList = [.. cvmList.Where(c => (comingFromIndexGeneral)
                        ? c.Name.Contains(conceptName, StringComparison.CurrentCultureIgnoreCase) // In IndexGeneral, search for concept name in all concepts
                        : c.IsRootConcept || c.Name.Contains(conceptName, StringComparison.CurrentCultureIgnoreCase))]; // In IndexSpecific, also the rootconcept must be taken into account, even if the rootconcept name does not match the search term
                }
            }

            return [.. cvmList
                .OrderBy(x => x.Name)
                .Select(c => new ConceptDisplayDTO
                {
                    ConceptViewModel = c,
                    ConceptRelationViewList = processConceptRelation.GetByConceptID(c.Id)
                })];
        }

        public (int RootConceptID, int? CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptEditDTO editDto)
        {
            ConceptViewModel? existingConcept = Get(new ConceptualRelationshipSearchParameterModel
            {
                Id = [editDto.Id]
            }).Select(x => x.ConceptViewModel).FirstOrDefault();
            if (existingConcept == null)
            {
                return (editDto.GetRootConceptId(), editDto.CollectionAreaID, 404, "Error_Concept_NotFound");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                processTranslations.Update(
                    new TranslationDTO
                    {
                        TextToTranslate = editDto.Name,
                        EntityType = nameof(Concept),
                        EntityId = existingConcept.Id,
                        FieldName = nameof(ConceptViewModel.Name),
                        Abbreviation = editDto.Abbreviation,
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name),
                        IsTranslateable = editDto.IsTranslateable
                    });

                SyncRelationConnections(existingConcept, editDto.ConceptRelationList);

                transactionScope.Complete();
                return (existingConcept.GetRootConceptId(), null, 201, "Success_Concept_Updated");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ConceptualRelationshipProcessor/Update", new Dictionary<string, object>
                {
                    { "Concept", editDto }
                });
                return (existingConcept.GetRootConceptId(), null, 500, "Error_Unknown");
            }
        }

        private void ConnectRelationToConcept(int conceptId, ConceptRelationCreateDTO relation)
        {
            if (conceptId == relation.ToConceptId)
            {
                trackEvents.TrackError("ConceptualRelationshipProcessor/ConnectRelationToConcept: Concept cannot have a relation to itself.", new Dictionary<string, object>
                {
                    {"ConceptID", conceptId }
                });
                return;
            }

            ConceptRelationViewModel conceptRelation = new()
            {
                FromConceptID = conceptId,
                ToConceptID = relation.ToConceptId,
                RelationTypeInt = relation.RelationTypeInt,
                IsDirected = relation.RelationTypeInt == 1 // if RelationTypeInt is SubTermOf, the relation is directed; if it's Synonym, it's undirected
            };
            if (relation.RelationTypeInt is 0)
            {
                conceptRelation.IsDirected = false;
            }

            _ = processConceptRelation.Insert(conceptRelation);
        }
        private void SyncRelationConnections(ConceptViewModel existingConcept, List<ConceptRelationEditDTO> newConnections)
        {
            List<ConceptRelationViewModel> currentConnections = processConceptRelation.GetByConceptID(existingConcept.Id);

            foreach (ConceptRelationViewModel current in currentConnections)
            {
                ConceptRelationEditDTO? updated = newConnections.FirstOrDefault(x => x.ToConceptId == current.ToConceptID);

                if (updated == null)
                {
                    _ = processConceptRelation.Delete(current);
                }
                else if (updated.RelationTypeInt != current.RelationTypeInt)
                {
                    ConceptRelationViewModel updatedConceptRelation = new()
                    {
                        FromConceptID = current.FromConceptID,
                        ToConceptID = current.ToConceptID,
                        RelationTypeInt = updated.RelationTypeInt,
                        IsDirected = updated.RelationTypeInt == 1 // if RelationTypeInt is SubTermOf, the relation is directed; if it's Synonym, it's undirected
                    };
                    _ = processConceptRelation.Update(updatedConceptRelation);
                }
            }

            foreach (ConceptRelationEditDTO newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ToConceptID == newItem.ToConceptId);
                if (!exists)
                {
                    ConceptRelationCreateDTO createDTO = new()
                    {
                        ToConceptId = newItem.ToConceptId,
                        RelationTypeInt = newItem.RelationTypeInt
                    };
                    ConnectRelationToConcept(existingConcept.Id, createDTO);
                }
            }
        }

        //private List<string> GetSubConceptNames(int conceptId, ConceptualRelationshipSearchParameterModel originalParams)
        //{
        //    var searchParams = new ConceptualRelationshipSearchParameterModel
        //    {
        //        RootConceptID = [conceptId],
        //        // Übernehmen Sie nur die Filter, die auch für Sub-Konzepte gelten sollen
        //        CollectionAreaID = originalParams.CollectionAreaID,
        //        ConceptTypeInt = originalParams.ConceptTypeInt
        //        // ConceptName explizit NICHT übernehmen
        //    };

        //    var subConceptList = Get(searchParams);
        //    return [.. subConceptList.Select(x => x.ConceptViewModel.Name)];
        //}
    }
}
