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
        (int CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        (int CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        List<ConceptualRelationshipOperationParameterModel> GetWithPredicates(ConceptualRelationshipSearchParameterModel conceptSearchParameter);
        (int Statuscode, string StatusMessage) DeleteConcept(int conceptID);
    }

    public class ConceptualRelationshipProcessor(IUnitOfWork unitOfWork
        , IProcessConceptRelation processConceptRelation
        , DeeplTranslationService translationService
        , IProcessTranslations processTranslations) : IProcessConcept
    {
        public (int CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (conceptualRelationshipOperation.Concept.Id != 0 || conceptualRelationshipOperation.Concept.CollectionAreaID <= 0 || string.IsNullOrWhiteSpace(conceptualRelationshipOperation.Concept.ConceptName))
            {
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Error_CollectionAreaID_Missing");
            }

            ConceptualRelationshipSearchParameterModel searchParameterModel = new()
            {
                ConceptName = [conceptualRelationshipOperation.Concept.ConceptName],
                CollectionAreaID = [conceptualRelationshipOperation.Concept.CollectionAreaID]
            }
            List<int> entityIds = [.. unitOfWork.EntityTranslationRepository.Get(et =>
                et.EntityType == nameof(Concept) &&
                et.FieldName == nameof(Concept.ConceptName) &&
                et.TranslatedText == conceptualRelationshipOperation.Concept.ConceptName)
                .Select(et => et.EntityId).Distinct()];
            if (entityIds.Count > 0)
            {
                searchParameterModel.ConceptID = entityIds;
            }
            Concept? existingConcept = GetWithPredicates(searchParameterModel).Select(x => x.Concept).FirstOrDefault();
            if (existingConcept != null)
            {
                return (existingConcept.CollectionAreaID, 409, "Error_Concept_Exists");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                conceptualRelationshipOperation.Concept.ConceptName = translationService.SetIntoFallbackLanguage(conceptualRelationshipOperation.Concept.ConceptName);
                Concept newConcept = unitOfWork.ConceptRepository.Insert(conceptualRelationshipOperation.Concept);
                unitOfWork.Save();

                foreach (ConceptRelation relation in conceptualRelationshipOperation.ConceptRelationList)
                {
                    ConnectRelationToConcept(newConcept, relation);
                }

                transactionScope.Complete();
                return (newConcept.CollectionAreaID, 201, "Success_Concept_Created");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 500, "Error_Error_Ocurred");
            }
        }

        public (int Statuscode, string StatusMessage) DeleteConcept(int conceptID)
        {
            Concept? existingConcept = GetWithPredicates(new ConceptualRelationshipSearchParameterModel { ConceptID = [conceptID] }).FirstOrDefault()?.Concept;
            if (existingConcept == null)
            {
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
                return (500, ex.Message);
            }
        }

        public List<ConceptualRelationshipOperationParameterModel> GetWithPredicates(ConceptualRelationshipSearchParameterModel conceptSearchParameter)
        {
            IEnumerable<Concept> query = unitOfWork.ConceptRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Concept>(conceptSearchParameter),
                includeProperties: "CollectionArea," +
                "CollectionItemEntityList");

            return [.. query
                .OrderBy(x => x.ConceptName)
                .Select(c => new ConceptualRelationshipOperationParameterModel
                {
                    Concept = c,
                    ConceptRelationList = processConceptRelation.GetByConceptID(c.Id)
                })];
        }

        public (int CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (conceptualRelationshipOperation.Concept.Id == 0)
            {
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Error_Concept_IDMissing");
            }
            if (conceptualRelationshipOperation.Concept.CollectionAreaID <= 0)
            {
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Error_CollectionArea_IdMissing");
            }
            if (string.IsNullOrWhiteSpace(conceptualRelationshipOperation.Concept.ConceptName))
            {
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Error_Concept_NameMissing");
            }

            Concept? existingConcept = unitOfWork.ConceptRepository.Get(c =>
                c.Id == conceptualRelationshipOperation.Concept.Id).FirstOrDefault();
            if (existingConcept == null)
            {
                return (0, 404, "Error_Concept_NotFound");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                bool isChanged = false;
                bool translatedTextExists = GetWithPredicates(new ConceptualRelationshipSearchParameterModel
                {
                    ConceptName = [conceptualRelationshipOperation.Concept.ConceptName],
                    ConceptID = [conceptualRelationshipOperation.Concept.Id],
                    CollectionAreaID = [conceptualRelationshipOperation.Concept.CollectionAreaID]
                }).Count != 0;
                if (translatedTextExists || existingConcept.ConceptName != conceptualRelationshipOperation.Concept.ConceptName)
                {
                    existingConcept.ConceptName = translationService.SetIntoFallbackLanguage(conceptualRelationshipOperation.Concept.ConceptName);
                    processTranslations.Edit(
                        new EntityTranslation
                        {
                            EntityType = nameof(Concept),
                            EntityId = existingConcept.Id,
                            FieldName = nameof(existingConcept.ConceptName),
                            TranslatedText = conceptualRelationshipOperation.Concept.ConceptName,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        conceptualRelationshipOperation.Concept.ConceptName);
                    isChanged = true;
                }
                if( existingConcept.Description != conceptualRelationshipOperation.Concept.Description)
                {
                    existingConcept.Description = conceptualRelationshipOperation.Concept.Description;
                    processTranslations.Edit(
                        new EntityTranslation
                        {
                            EntityType = nameof(Concept),
                            EntityId = existingConcept.Id,
                            FieldName = nameof(existingConcept.Description),
                            TranslatedText = conceptualRelationshipOperation.Concept.Description,
                            Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                        },
                        conceptualRelationshipOperation.Concept.Description);
                    isChanged = true;
                }
                if (isChanged)
                {
                    unitOfWork.Save();
                }

                SyncRelationConnections(existingConcept, conceptualRelationshipOperation.ConceptRelationList);

                transactionScope.Complete();
                return (existingConcept.CollectionAreaID, 201, "Success_Concept_Updated");
            }
            catch (Exception ex)
            {
                return (existingConcept.CollectionAreaID, 500, ex.Message);
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
