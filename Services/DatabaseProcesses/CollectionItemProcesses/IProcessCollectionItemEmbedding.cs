using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
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
        , IEmbeddingService embeddingService) : IProcessCollectionItemEmbedding
    {
        public (int Statuscode, string Statusmessage) Insert(CollectionItemEntity collectionItemEntity, List<string> translatedTeextList)
        {
            if(collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var vectors = GenerateAllEmbeddings(collectionItemEntity);
            foreach (var text in translatedTeextList)
            {
                var vector = embeddingService.GenerateEmbedding(text);
                vectors.Add(vector);
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

        public (int Statuscode, string Statusmessage) Update(CollectionItemEntity collectionItemEntity, List<string> translatedTeextList)
        {
            if (collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var existingEmbedding = unitOfWork.CollectionItemEmbeddingRepository.Get(x => x.CollectionItemEntityID == collectionItemEntity.CollectionItemEntityID, includeProperties:"CollectionItemEntity").FirstOrDefault();

            var vectors = GenerateAllEmbeddings(collectionItemEntity);
            foreach (var text in translatedTeextList)
            {
                var vector = embeddingService.GenerateEmbedding(text);
                vectors.Add(vector);
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
                return (200, "Error_Embedding_Missing");
            }

            unitOfWork.CollectionItemEmbeddingRepository.Delete(existingEmbedding);
            unitOfWork.Save();
            return (200, "Success_Embedding_Deleted");
        }

        private List<float[]> GenerateAllEmbeddings(CollectionItemEntity item)
        {
            var vectors = new List<float[]>();

            AddTextEmbedding(vectors, item.CollectionArea.CollectionAreaName);
            if(item.ConceptValueList != null)
            {
                foreach (var value in item.ConceptValueList)
                {
                    AddTextEmbedding(vectors, value.Concept.Name);
                    AddTextEmbedding(vectors, value.ValueDisplay);
                }
            }
            AddTextEmbedding(vectors, item.StatePreservation?.StatePreservationName);
            AddTextEmbedding(vectors, item.UniqueName);
            AddTextEmbedding(vectors, item.Comment);
            AddTextEmbedding(vectors, item.Inscription);
            AddTextEmbedding(vectors, item.PersonalIdentificationNumber);
            AddTextEmbedding(vectors, item.SerialNumber);
            AddTextEmbedding(vectors, item.Time);

            if (item.CollectionItemNPartyList?.Count > 0)
            {
                var partyText = string.Join(", ", item.CollectionItemNPartyList
                    .Select(p => p.Party?.PartyName ?? ""));
                AddTextEmbedding(vectors, partyText);
                var pseudonymText = string.Join(", ", item.CollectionItemNPartyList
                    .Select(p => p.Party?.Individual?.Pseudonym ?? ""));
                AddTextEmbedding(vectors, pseudonymText);
                var signature = string.Join(", ", item.CollectionItemNPartyList
                    .Select(p => p.Party?.Individual?.Signature ?? ""));
                AddTextEmbedding(vectors, signature);
                var productionFacility = string.Join(", ", item.CollectionItemNPartyList
                    .Select(p => p.Party?.Organization?.ProductionFacility?.ProductionFacilityName ?? ""));
                AddTextEmbedding(vectors, productionFacility);
                var organizationType = string.Join(", ", item.CollectionItemNPartyList
                    .Select(p => p.Party?.Organization?.OrganizationTypeEnum.ToString() ?? ""));
                AddTextEmbedding(vectors, organizationType);
            }

            if (item.CollectionItemNPlaceList?.Count > 0)
            {
                var placeText = string.Join(", ", item.CollectionItemNPlaceList
                    .Select(p => p.Place?.PlaceNToponymyList.FirstOrDefault(x => x.IsCurrentName)?.Toponymy.ToponymyName?? ""));
                AddTextEmbedding(vectors, placeText);
            }

            AddTextEmbedding(vectors, item.Concept?.Name);
            AddTextEmbedding(vectors, item.Era?.EraName);

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
