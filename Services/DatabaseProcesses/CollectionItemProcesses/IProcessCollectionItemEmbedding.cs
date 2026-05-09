using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;
using Sammlerplattform.Services.DatabaseProcesses.ConceptualRelationshipProcesses;
using Sammlerplattform.Services.ML.VectorSearch;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionItemEmbedding
    {
        (int Statuscode, string Statusmessage) Insert(CollectionItemEntity collectionItemEntity, List<string> translatedTeextList);
        (int Statuscode, string Statusmessage) Update(CollectionItemEntity collectionItemEntity, List<string> translatedTeextList);
        (int Statuscode, string Statusmessage) Delete(int collectionItemEntityID);
        List<CollectionItemSearchResultDTO> Search(string query);
    }
    public class CollectionItemEmbeddingProcessor(IUnitOfWork unitOfWork
        , IEmbeddingService embeddingService
        , IProcessCollectionArea processCollectionArea
        , IProcessEra processEra
        , IProcessConcept processConcept) : IProcessCollectionItemEmbedding
    {
        public (int Statuscode, string Statusmessage) Insert(CollectionItemEntity collectionItemEntity, List<string> translatedTextList)
        {
            if (collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var vectors = AddCollectionItemWOTranslationsEmbeddings(collectionItemEntity);
            foreach (var text in translatedTextList)
            {
                AddTextEmbedding(vectors, text);
            }
            var combinedVector = embeddingService.CombineVectors(vectors);
            unitOfWork.CollectionItemEmbeddingRepository.Insert(new CollectionItemEmbedding
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                CombinedEmbedding = combinedVector,
                LastUpdated = DateTime.UtcNow
            });
            unitOfWork.Save();

            return (200, "Embedding created successfully");
        }

        public (int Statuscode, string Statusmessage) Update(CollectionItemEntity collectionItemEntity, List<string> translatedTextList)
        {
            if (collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var existingEmbedding = unitOfWork.CollectionItemEmbeddingRepository.Get(x => x.CollectionItemEntityID == collectionItemEntity.CollectionItemEntityID, includeProperties: "CollectionItemEntity").FirstOrDefault();

            var vectors = AddCollectionItemWOTranslationsEmbeddings(collectionItemEntity);
            foreach (var text in translatedTextList)
            {
                AddTextEmbedding(vectors, text);
            }
            var combinedVector = embeddingService.CombineVectors(vectors);

            if (existingEmbedding == null)
            {
                var newEmbedding = new CollectionItemEmbedding
                {
                    CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                    CombinedEmbedding = combinedVector,
                    LastUpdated = DateTime.UtcNow
                };
                unitOfWork.CollectionItemEmbeddingRepository.Insert(newEmbedding);
            }
            else
            {
                existingEmbedding.CombinedEmbedding = combinedVector;
                existingEmbedding.LastUpdated = DateTime.UtcNow;
            }
            unitOfWork.Save();

            return (200, "Success_Embedding_Updated");
        }

        public (int Statuscode, string Statusmessage) Delete(int collectionItemEntityID)
        {
            var existingEmbedding = unitOfWork.CollectionItemEmbeddingRepository.Get(x => x.CollectionItemEntityID == collectionItemEntityID, includeProperties: "CollectionItemEntity").FirstOrDefault();
            if (existingEmbedding == null)
            {
                return (404, "Error_Embedding_NotFound");
            }

            unitOfWork.CollectionItemEmbeddingRepository.Delete(existingEmbedding);
            unitOfWork.Save();
            return (200, "Success_Embedding_Deleted");
        }

        private List<float[]> AddCollectionItemWOTranslationsEmbeddings(CollectionItemEntity item)
        {
            var vectors = new List<float[]>();

            AddTextEmbedding(vectors, processCollectionArea.GetListWithPredicate(new Models.CollectionAreaDatabase.CollectionAreaSearchParameterModel
            { CollectionAreaID = [item.CollectionAreaID] }).FirstOrDefault()?.CollectionAreaName);
            foreach (var value in item.ConceptValueList)
            {
                AddTextEmbedding(vectors, processConcept.Get(new Models.ConceptualRelationshipDatabase.ConceptualRelationshipSearchParameterModel
                { Id = [value.ConceptID] }).FirstOrDefault()?.ConceptViewModel.Name);
                AddTextEmbedding(vectors, value.ValueString);
                AddTextEmbedding(vectors, value.ValueDate.ToString());
            }
            AddTextEmbedding(vectors, item.StatePreservation?.StatePreservationName);
            AddTextEmbedding(vectors, item.Inscription);
            AddTextEmbedding(vectors, item.Time);

            if (item.CollectionItemNParticipantList?.Count > 0)
            {
                var participantText = string.Join(", ", item.CollectionItemNParticipantList
                    .Select(p => p.Participant?.ParticipantName ?? ""));
                AddTextEmbedding(vectors, participantText);
                var pseudonymText = string.Join(", ", item.CollectionItemNParticipantList
                    .Select(p => p.Participant?.Individual?.Pseudonym ?? ""));
                AddTextEmbedding(vectors, pseudonymText);
                var signature = string.Join(", ", item.CollectionItemNParticipantList
                    .Select(p => p.Participant?.Individual?.Signature ?? ""));
                AddTextEmbedding(vectors, signature);
                var industry = string.Join(", ", item.CollectionItemNParticipantList
                    .Select(p => p.Participant?.Organization?.Industry?.IndustryName ?? ""));
                AddTextEmbedding(vectors, industry);
            }

            if (item.CollectionItemNPlaceList?.Count > 0)
            {
                var placeText = string.Join(", ", item.CollectionItemNPlaceList
                    .Select(p => p.Place?.PlaceNToponymyList.FirstOrDefault(x => x.IsCurrentName)?.Toponymy.ToponymyName ?? ""));
                AddTextEmbedding(vectors, placeText);
            }
            if (item.EraID != null)
            {
                AddTextEmbedding(vectors, processEra.GetWithPredicates(new Models.EraDatabase.EraSearchParameterModel
                { EraID = [(int)item.EraID] }).FirstOrDefault()?.EraName);
            }

            return vectors;
        }

        private void AddTextEmbedding(List<float[]> vectors, string? text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                vectors.Add(embeddingService.GenerateEmbedding(text));
            }
        }

        public List<CollectionItemSearchResultDTO> Search(string query)
        {
            var queryEmbedding = embeddingService.GenerateEmbedding(query);

            // Da SQL Server keine native Vektor-Cosine-Similarity hat:
            // 1. Alle Embeddings laden und im Memory berechnen
            var allEmbeddings = unitOfWork.CollectionItemEmbeddingRepository.Get(
                includeProperties: nameof(CollectionItemEmbedding.CollectionItemEntity)
            ).ToList();

            var results = allEmbeddings
                .Select(e => new CollectionItemSearchResultDTO
                {
                    CollectionItemEntityID = e.CollectionItemEntityID,
                    CosineSimilarity = CalculateCosineSimilarity(e.CombinedEmbedding, queryEmbedding),
                    EuclideanSimilarity = CalculateEuclideanSimilarity(e.CombinedEmbedding, queryEmbedding)
                })
                .Where(x => x.CosineSimilarity > 0.3 || x.EuclideanSimilarity > 0.3) // Minimum threshold
                .OrderByDescending(x => x.CosineSimilarity).ThenByDescending(x => x.EuclideanSimilarity)
                .ToList();

            return results;
        }

        private static double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            double dotProduct = 0.0, magnitude1 = 0.0, magnitude2 = 0.0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            return dotProduct / (magnitude1 * magnitude2);
        }
        private static double CalculateEuclideanSimilarity(float[] v1, float[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                sum += Math.Pow(v1[i] - v2[i], 2);
            }
            return 1 / (1 + Math.Sqrt(sum)); // Convert distance to similarity
        }
    }
}
