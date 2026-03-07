using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses
{
    public interface IProcessConceptRelation
    {
        int Insert(ConceptRelationViewModel conceptRelation);
        int Update(ConceptRelationViewModel conceptRelation);
        int Delete(ConceptRelationViewModel conceptRelation);
        List<ConceptRelationViewModel> GetByRootConceptID(int conceptID);
        List<ConceptRelationViewModel> GetByConceptID(int conceptID);

    }

    public class ConceptRelationProcessor(DbIdentityContext dbIdentityContext,
        IProcessTranslations processTranslation,
        IDeeplTranslationService deeplTranslationService) : IProcessConceptRelation
    {
        public int Delete(ConceptRelationViewModel conceptRelation)
        {
            string sql = @"
                        DELETE r
                        FROM ConceptRelation AS r
                        WHERE EXISTS (
                            SELECT 1 
                            FROM Concept c1, Concept c2
                            WHERE MATCH (c1-(r)->c2)
                              AND c1.Id = @p0
                              AND c2.Id = @p1
                        )";

            int returncode = dbIdentityContext.Database.ExecuteSqlRaw(sql, conceptRelation.FromConceptID, conceptRelation.ToConceptID);
            return returncode;
        }

        public List<ConceptRelationViewModel> GetByConceptID(int conceptID)
        {
            return GetConceptRelations(
                $@"
                SELECT c1.Id AS FromConceptID, 
                c2.Id AS ToConceptID, 
                r.RelationTypeInt, 
                r.IsDirected 
                FROM Concept c1, ConceptRelation r, Concept c2 
                WHERE MATCH (c1-(r)->c2)
                AND c1.Id = @p0",
                conceptID
            );
        }

        public List<ConceptRelationViewModel> GetByRootConceptID(int rootConceptID)
        {
            return GetConceptRelations(
                $@"
                SELECT c1.Id AS FromConceptID, 
                c2.Id AS ToConceptID, 
                r.RelationTypeInt, 
                r.IsDirected 
                FROM Concept c1, ConceptRelation r, Concept c2 
                WHERE MATCH (c1-(r)->c2)
                AND c1.RootConceptID = @p0",
                rootConceptID
            );
        }

        private List<ConceptRelationViewModel> GetConceptRelations(string sqlQuery, int parameter)
        {
            List<ConceptRelationViewModel> conceptRelationList = [.. dbIdentityContext.ConceptRelation.FromSqlRaw(sqlQuery, parameter)];

            foreach (var conceptRelation in conceptRelationList)
            {
                ConceptViewModel conceptView = new()
                {
                    Name = processTranslation.GetWithPredicate(new EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(Concept)],
                        FieldName = [nameof(ConceptViewModel.Name)],
                        EntityId = [conceptRelation.ToConceptID],
                        Culture = [deeplTranslationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)]
                    }).Select(x => x.TranslatedText).FirstOrDefault() ?? ""
                };
                conceptRelation.ToConcept = conceptView;
            }

            return conceptRelationList;
        }

        public int Insert(ConceptRelationViewModel conceptRelation)
        {
            string sql = @"
                INSERT INTO ConceptRelation (RelationTypeInt, $from_id, $to_id, IsDirected)
                VALUES (@p0,
                (SELECT $node_id FROM Concept WHERE Id = @p1),
                (SELECT $node_id FROM Concept WHERE Id = @p2),
                @p3)";

            int returncode = dbIdentityContext.Database.ExecuteSqlRaw(sql, conceptRelation.RelationTypeInt, conceptRelation.FromConceptID, conceptRelation.ToConceptID, conceptRelation.IsDirected);

            return returncode;
        }

        public int Update(ConceptRelationViewModel conceptRelation)
        {
            string sql = @"
                UPDATE ConceptRelation r
                SET RelationTypeInt = @p2, IsDirected = @p3
                WHERE MATCH ((c1)-[r]->(c2))
                  AND c1.Id = @p0
                  AND c2.Id = @p1";
            int returncode = dbIdentityContext.Database.ExecuteSqlRaw(sql,
                conceptRelation.FromConceptID,
                conceptRelation.ToConceptID,
                conceptRelation.RelationTypeInt,
                conceptRelation.IsDirected);

            return returncode;
        }
    }
}
