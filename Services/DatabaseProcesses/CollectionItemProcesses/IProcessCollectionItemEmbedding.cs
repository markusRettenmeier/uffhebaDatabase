using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Services.ML.VectorSearch;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCollectionItemEmbedding
    {
        (int Statuscode, string Statusmessage) Insert(CollectionItemDisplayDTO collectionItemEntity, Dictionary<string, string> translatedTextList);
        (int Statuscode, string Statusmessage) Update(CollectionItemDisplayDTO collectionItemEntity, Dictionary<string, string> translatedTextList);
        (int Statuscode, string Statusmessage) Delete(int collectionItemEntityID);
        Task<List<CollectionItemSearchResultDTO>> SearchAsync(string query);
    }
    public class CollectionItemEmbeddingProcessor(IUnitOfWork unitOfWork
        , M3Embedder m3Embedder) : IProcessCollectionItemEmbedding
    {
        public (int Statuscode, string Statusmessage) Insert(CollectionItemDisplayDTO collectionItemEntity, Dictionary<string, string> translatedTextList)
        {
            if (collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var textToEmbed = CombineTranslations(collectionItemEntity, translatedTextList);
            var generateEmbeddings = m3Embedder.GenerateEmbeddings(textToEmbed);
            unitOfWork.CollectionItemEmbeddingRepository.Insert(new CollectionItemEmbedding
            {
                CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                HandleSparseWeigths = generateEmbeddings.SparseWeights,
                DenseEmbedding = generateEmbeddings.DenseEmbedding,
                LastUpdated = DateTime.UtcNow
            });
            unitOfWork.Save();

            return (200, "Embedding created successfully");
        }

        public (int Statuscode, string Statusmessage) Update(CollectionItemDisplayDTO collectionItemEntity, Dictionary<string, string> translatedTextList)
        {
            if (collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var existingEmbedding = unitOfWork.CollectionItemEmbeddingRepository.Get(
                filter: x => x.CollectionItemEntityID == collectionItemEntity.CollectionItemEntityID,
                includeProperties: "CollectionItemEntity").FirstOrDefault();

            var textToEmbed = CombineTranslations(collectionItemEntity, translatedTextList);
            var generateEmbeddings = m3Embedder.GenerateEmbeddings(textToEmbed);

            if (existingEmbedding == null)
            {
                var newEmbedding = new CollectionItemEmbedding
                {
                    CollectionItemEntityID = collectionItemEntity.CollectionItemEntityID,
                    DenseEmbedding = generateEmbeddings.DenseEmbedding,
                    HandleSparseWeigths = generateEmbeddings.SparseWeights,
                    LastUpdated = DateTime.UtcNow
                };
                unitOfWork.CollectionItemEmbeddingRepository.Insert(newEmbedding);
            }
            else
            {
                existingEmbedding.HandleSparseWeigths = generateEmbeddings.SparseWeights;
                existingEmbedding.DenseEmbedding = generateEmbeddings.DenseEmbedding;
                existingEmbedding.LastUpdated = DateTime.UtcNow;
            }
            unitOfWork.Save();

            return (200, "Success_Embedding_Updated");
        }

        public (int Statuscode, string Statusmessage) Delete(int collectionItemEntityID)
        {
            var existingEmbedding = unitOfWork.CollectionItemEmbeddingRepository.Get(x => x.CollectionItemEntityID == collectionItemEntityID, 
                includeProperties: nameof(CollectionItemEmbedding.CollectionItemEntity)).FirstOrDefault();
            if (existingEmbedding == null)
            {
                return (404, "Error_Embedding_NotFound");
            }

            unitOfWork.CollectionItemEmbeddingRepository.Delete(existingEmbedding);
            unitOfWork.Save();

            return (200, "Success_Embedding_Deleted");
        }

        private static string CombineTranslations(CollectionItemDisplayDTO item, Dictionary<string, string> translatedTextList)
        {
            string combinedtext = string.Empty;
            foreach (var kvp in translatedTextList)
            {
                combinedtext += " " + kvp.Key + ": " + kvp.Value;
            }
            combinedtext += nameof(CollectionItemDisplayDTO.CollectionAreaName) + ": " + item.CollectionAreaName + "; ";
            combinedtext += nameof(CollectionItemDisplayDTO.Inscription) + ": " + item.Inscription + "; ";
            combinedtext += nameof(CollectionItemDisplayDTO.Time) + ": " + item.Time + "; ";
            combinedtext += nameof(Participant.ParticipantName) + ": " + string.Join(", ", item.CollectionItemNParticipantList
                .Select(p => p.Name ?? "")) + "; ";
            combinedtext += nameof(Toponymy.ToponymyName) + ": " + string.Join(", ", item.CollectionItemNPlaceList
                .Select(p => p.ToponymyList.ToList().Select(x => x.Name))) + "; ";
            combinedtext += nameof(CollectionItemDisplayDTO.Fake) + ": " + item.Fake + "; ";
            combinedtext += nameof(CollectionItemDisplayDTO.SerialNumber) + ": " + item.SerialNumber + "; ";

            return combinedtext;
        }

        public async Task<List<CollectionItemSearchResultDTO>> SearchAsync(string query)
        {
            var queryEmbedding = m3Embedder.GenerateEmbeddings(query);

            var querySparse = queryEmbedding.SparseWeights;

            var allEmbeddings = unitOfWork.CollectionItemEmbeddingRepository
                .Get(includeProperties:
                    nameof(CollectionItemEmbedding.CollectionItemEntity))
                .ToList();

            var results = allEmbeddings
                .Select(e =>
                {
                    var denseScore =
                        CalculateCosineSimilarity(
                            e.DenseEmbedding,
                            queryEmbedding.DenseEmbedding);

                    var sparseScore =
                        CalculateSparseCosineSimilarity(
                            querySparse,
                            e.HandleSparseWeigths);

                    var hybridScore =
                        (denseScore * 0.6) +
                        (sparseScore * 0.4);

                    return new CollectionItemSearchResultDTO
                    {
                        CollectionItemEntityID = e.CollectionItemEntityID,
                        CosineSimilarity = denseScore,
                        SparseSimilarity = sparseScore,
                        HybridScore = hybridScore
                    };
                })
                .OrderByDescending(x => x.HybridScore)
                .Take(100)
                .ToList();

            return results;
        }

        private static double CalculateSparseCosineSimilarity(
                Dictionary<int, float> queryWeights,
                Dictionary<int, float> documentWeights)
        {
            double dotProduct = 0;
            double queryNorm = 0;
            double docNorm = 0;

            foreach (var q in queryWeights)
            {
                queryNorm += q.Value * q.Value;

                if (documentWeights.TryGetValue(q.Key, out var docWeight))
                {
                    dotProduct += q.Value * docWeight;
                }
            }

            foreach (var d in documentWeights)
            {
                docNorm += d.Value * d.Value;
            }

            queryNorm = Math.Sqrt(queryNorm);
            docNorm = Math.Sqrt(docNorm);

            if (queryNorm == 0 || docNorm == 0)
                return 0;

            return dotProduct / (queryNorm * docNorm);
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
    }
}
