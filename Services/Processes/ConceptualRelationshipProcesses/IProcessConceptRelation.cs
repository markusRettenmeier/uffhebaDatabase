using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;

namespace Sammlerplattform.Services.Processes.ConceptualRelationshipProcesses
{
    public interface IProcessConceptRelation
    {
        int Insert(ConceptRelation conceptRelation);
        int Update(ConceptRelation conceptRelation);
        int Delete(ConceptRelation conceptRelation);
        List<ConceptRelation> GetByConceptID(int conceptID);
        List<ConceptRelation> GetByCollectionAreaID(int collectionAreaID);

    }

    public class ConceptRelationProcessor(DbIdentityContext dbIdentityContext, IUnitOfWork unitOfWork) : IProcessConceptRelation
    {
        public int Delete(ConceptRelation conceptRelation)
        {
            string sql = @"
        DELETE r
        FROM ConceptRelation AS r
        WHERE EXISTS (
            SELECT 1 
            FROM Concept c1, Concept c2
            WHERE MATCH (c1-(r)->c2)
              AND c1.ConceptID = @p0
              AND c2.ConceptID = @p1
        )";

            int returncode = dbIdentityContext.Database.ExecuteSqlRaw(sql, conceptRelation.FromConceptID, conceptRelation.ToConceptID);
            return returncode;
        }

        //public List<ConceptRelation> Get(int collectionAreaID)
        //{
        //    List<ConceptRelation> existingConceptRelation =
        //        [.. dbIdentityContext.ConceptRelationshipQueryResult
        //        .FromSqlRaw(@"
        //                SELECT 
        //                    c1.ConceptID AS FromId,
        //                    c2.ConceptID AS ToId,
        //                    r.RelationTypeInt AS RelationshipInt,
        //                    r.IsDirected
        //                FROM Concept c1, ConceptRelation r, Concept c2
        //                WHERE c1.CollectionAreaID = @p0
        //                  AND MATCH (c1-(r)->c2)",
        //            collectionAreaID)
        //        .Select(x => new ConceptRelation
        //        {
        //            RelationTypeInt = x.RelationshipInt,
        //            FromConceptID = x.FromId,
        //            ToConceptID = x.ToId,
        //            ToConcept = unitOfWork.ConceptRepository.GetByID(x.ToId)!,
        //            IsDirected = x.IsDirected
        //        })];
        //    return existingConceptRelation;
        //}
        public List<ConceptRelation> GetByCollectionAreaID(int collectionAreaID)
        {
            return GetConceptRelations(
                @"SELECT 
            c1.ConceptID AS FromId,
            c2.ConceptID AS ToId,
            r.RelationTypeInt AS RelationshipInt,
            r.IsDirected
        FROM Concept c1, ConceptRelation r, Concept c2
        WHERE c1.CollectionAreaID = @p0
          AND MATCH (c1-(r)->c2)",
                collectionAreaID
            );
        }

        public List<ConceptRelation> GetByConceptID(int conceptID)
        {
            return GetConceptRelations(
                @"SELECT 
            c1.ConceptID AS FromId,
            c2.ConceptID AS ToId,
            r.RelationTypeInt AS RelationshipInt,
            r.IsDirected
        FROM Concept c1, ConceptRelation r, Concept c2
        WHERE c1.ConceptID = @p0
          AND MATCH (c1-(r)->c2)",
                conceptID
            );
        }

        private List<ConceptRelation> GetConceptRelations(string sqlQuery, int parameter)
        {
            List<ConceptRelationshipQueryResult> conceptRelations = [.. dbIdentityContext.ConceptRelationshipQueryResult.FromSqlRaw(sqlQuery, parameter)];

            List<int> toConceptIds = [.. conceptRelations.Select(x => x.ToId).Distinct()];

            // Batch-Loading für bessere Performance
            Dictionary<int, Concept> concepts = unitOfWork.ConceptRepository
                .Get(filter: c => toConceptIds.Contains(c.ConceptID))
                .ToDictionary(c => c.ConceptID);

            List<ConceptRelation> result = [];

            foreach (ConceptRelationshipQueryResult? x in conceptRelations)
            {
                if (!concepts.TryGetValue(x.ToId, out Concept? toConcept))
                {
                    throw new InvalidOperationException($"Concept mit ID {x.ToId} nicht gefunden");
                }

                result.Add(new ConceptRelation
                {
                    RelationTypeInt = x.RelationshipInt,
                    FromConceptID = x.FromId,
                    ToConceptID = x.ToId,
                    ToConcept = toConcept,
                    IsDirected = x.IsDirected
                });
            }

            return result;
        }

        public int Insert(ConceptRelation conceptRelation)
        {
            string sql = @"
                INSERT INTO ConceptRelation (RelationTypeInt, $from_id, $to_id, IsDirected)
                VALUES (@p0,
                (SELECT $node_id FROM Concept WHERE ConceptID = @p1),
                (SELECT $node_id FROM Concept WHERE ConceptID = @p2),
                @p3)";

            int returncode = dbIdentityContext.Database.ExecuteSqlRaw(sql, conceptRelation.RelationTypeInt, conceptRelation.FromConceptID, conceptRelation.ToConceptID, conceptRelation.IsDirected);

            return returncode;
        }

        public int Update(ConceptRelation conceptRelation)
        {
            string sql = @"
                UPDATE ConceptRelation r
                SET RelationTypeInt = @p2, IsDirected = @p3
                WHERE MATCH ((c1)-[r]->(c2))
                  AND c1.ConceptID = @p0
                  AND c2.ConceptID = @p1";
            int returncode = dbIdentityContext.Database.ExecuteSqlRaw(sql,
                conceptRelation.FromConceptID,
                conceptRelation.ToConceptID,
                conceptRelation.RelationTypeInt,
                conceptRelation.IsDirected);

            return returncode;
        }
    }
}
