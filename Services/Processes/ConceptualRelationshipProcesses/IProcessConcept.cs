using Sammlerplattform.Data;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.ConceptualRelationshipProcesses
{
    public interface IProcessConcept
    {
        (int CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        (int CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation);
        List<ConceptualRelationshipOperationParameterModel> GetWithPredicates(ConceptualRelationshipSearchParameterModel conceptSearchParameter);
        (int Statuscode, string StatusMessage) DeleteConcept(int conceptID);
    }

    public class ConceptualRelationshipProcessor(IUnitOfWork unitOfWork, IProcessConceptRelation processConceptRelation) : IProcessConcept
    {
        public (int CollectionAreaID, int Statuscode, string StatusMessage) Insert(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (conceptualRelationshipOperation.Concept.ConceptID != 0 || conceptualRelationshipOperation.Concept.CollectionAreaID <= 0 || string.IsNullOrWhiteSpace(conceptualRelationshipOperation.Concept.ConceptName))
            {
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Invalid input parameters.");
            }

            Concept? existingConcept = unitOfWork.ConceptRepository.Get(c =>
                c.ConceptName == conceptualRelationshipOperation.Concept.ConceptName &&
                c.CollectionAreaID == conceptualRelationshipOperation.Concept.CollectionAreaID).FirstOrDefault();
            if (existingConcept != null)
            {
                return (existingConcept.CollectionAreaID, 409, "Concept already exists.");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                Concept newConcept = unitOfWork.ConceptRepository.Insert(conceptualRelationshipOperation.Concept);
                unitOfWork.Save();

                foreach (ConceptRelation relation in conceptualRelationshipOperation.ConceptRelationList)
                {
                    ConnectRelationToConcept(newConcept, relation);
                }

                transactionScope.Complete();
                return (newConcept.CollectionAreaID, 201, "Beziehung erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (int Statuscode, string StatusMessage) DeleteConcept(int conceptID)
        {
            Concept? existingConcept = GetWithPredicates(new ConceptualRelationshipSearchParameterModel { ConceptID = [conceptID] }).FirstOrDefault()?.Concept;
            if (existingConcept == null)
            {
                return (404, "Beziehung nicht gefunden");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                List<ConceptRelation> existingRelations = processConceptRelation.GetByConceptID(existingConcept.ConceptID);
                for (int i = 0; i < existingRelations.Count; i++)
                {
                    _ = processConceptRelation.Delete(existingRelations[i]);
                }

                unitOfWork.ConceptRepository.Delete(existingConcept);
                unitOfWork.Save();

                transactionScope.Complete();
                return (200, "Beziehung erfolgreich gelöscht.");
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
                    ConceptRelationList = processConceptRelation.GetByConceptID(c.ConceptID)
                })];
        }

        public (int CollectionAreaID, int Statuscode, string StatusMessage) Update(ConceptualRelationshipOperationParameterModel conceptualRelationshipOperation)
        {
            if (conceptualRelationshipOperation.Concept.ConceptID == 0 || conceptualRelationshipOperation.Concept.CollectionAreaID <= 0 || string.IsNullOrWhiteSpace(conceptualRelationshipOperation.Concept.ConceptName))
            {
                return (conceptualRelationshipOperation.Concept.CollectionAreaID, 400, "Invalid input parameters.");
            }

            Concept? existingConcept = unitOfWork.ConceptRepository.Get(c =>
                c.ConceptID == conceptualRelationshipOperation.Concept.ConceptID).FirstOrDefault();
            if (existingConcept == null)
            {
                return (0, 404, "Beziehung nicht gefunden");
            }

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (existingConcept.ConceptName != conceptualRelationshipOperation.Concept.ConceptName ||
                    existingConcept.Description != conceptualRelationshipOperation.Concept.Description)
                {
                    existingConcept.ConceptName = conceptualRelationshipOperation.Concept.ConceptName;
                    existingConcept.Description = conceptualRelationshipOperation.Concept.Description;
                    unitOfWork.Save();
                }

                SyncRelationConnections(existingConcept, conceptualRelationshipOperation.ConceptRelationList);

                transactionScope.Complete();
                return (existingConcept.CollectionAreaID, 201, "Beziehung erfolgreich erstellt.");
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

            relation.FromConceptID = newConcept.ConceptID; // Ensure FromConceptID is set to the newly created concept
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
            List<ConceptRelation> currentConnections = processConceptRelation.GetByConceptID(existingConcept.ConceptID);

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
        //public async Task AddRelationAsync(int fromId, int toId, int relationType, bool isDirected)
        //{
        //    var sql = @"
        //        INSERT INTO ConceptRelation (RelationTypeInt, $from_id, $to_id, IsDirected)
        //        VALUES (@p0,
        //        (SELECT $node_id FROM Concept WHERE ConceptID = @p1),
        //        (SELECT $node_id FROM Concept WHERE ConceptID = @p2),
        //        @p3)";

        //    await dbContext.Database.ExecuteSqlRawAsync(sql, relationType, fromId, toId, isDirected);
        //}
    }
}
