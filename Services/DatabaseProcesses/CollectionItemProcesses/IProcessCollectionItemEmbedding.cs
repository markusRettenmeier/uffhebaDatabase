using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionAreaDatabase;
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
        Task<List<CollectionItemSearchResultDTO>> SearchAsync(string query);
    }
    public class CollectionItemEmbeddingProcessor(IUnitOfWork unitOfWork
        , M3Embedder m3Embedder) : IProcessCollectionItemEmbedding
    {
        public (int Statuscode, string Statusmessage) Insert(CollectionItemEntity collectionItemEntity, List<string> translatedTextList)
        {
            if (collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var textToEmbed = CombineTranslations(collectionItemEntity);
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

        public (int Statuscode, string Statusmessage) Update(CollectionItemEntity collectionItemEntity, List<string> translatedTextList)
        {
            if (collectionItemEntity.CollectionItemEntityID == 0)
            {
                return (400, "Error_CollectionItemEntity_IDMissing");
            }

            var existingEmbedding = unitOfWork.CollectionItemEmbeddingRepository.Get(
                filter: x => x.CollectionItemEntityID == collectionItemEntity.CollectionItemEntityID
                , includeProperties: "CollectionItemEntity").FirstOrDefault();

            var textToEmbed = CombineTranslations(collectionItemEntity);
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
            var existingEmbedding = unitOfWork.CollectionItemEmbeddingRepository.Get(x => x.CollectionItemEntityID == collectionItemEntityID, includeProperties: "CollectionItemEntity").FirstOrDefault();
            if (existingEmbedding == null)
            {
                return (404, "Error_Embedding_NotFound");
            }

            unitOfWork.CollectionItemEmbeddingRepository.Delete(existingEmbedding);
            unitOfWork.Save();
            return (200, "Success_Embedding_Deleted");
        }

        private string CombineTranslations(CollectionItemEntity item)
        {
            string combinedtext = string.Empty;

            var translationList = unitOfWork.EntityTranslationRepository.Get(filter: x => x.EntityId == item.CollectionItemEntityID);
            foreach (var translation in translationList)
            {
                combinedtext += " " + translation.FieldName + " " + translation.TranslatedText;
                //AddTextEmbedding(new List<float[]>(), translation.TranslatedText);
            }

            combinedtext += nameof(CollectionArea.CollectionAreaName) + ": " + item.CollectionArea.CollectionAreaName + "; ";
            combinedtext += nameof(CollectionItemEntity.Inscription) + ": " + item.Inscription + "; ";
            combinedtext += nameof(CollectionItemEntity.Time) + ": " + item.Time + "; ";
            combinedtext += nameof(CollectionItemEntity.CollectionItemNParticipantList) + ": " + string.Join(", ", item.CollectionItemNParticipantList
                .Select(p => p.Participant?.ParticipantName ?? "")) + "; ";
            combinedtext += nameof(CollectionItemEntity.CollectionItemNPlaceList) + ": " + string.Join(", ", item.CollectionItemNPlaceList
                .Select(p => p.Place?.PlaceNToponymyList.FirstOrDefault(x => x.IsCurrentName)?.Toponymy.ToponymyName ?? "")) + "; ";
            combinedtext += nameof(CollectionItemEntity.ConceptValueList) + ": " + string.Join(", ", item.ConceptValueList
                .Select(v => v.ValueDisplay)) + "; ";
            combinedtext += nameof(CollectionItemEntity.Fake) + ": " + item.Fake + "; ";
            combinedtext += nameof(CollectionItemEntity.SerialNumber) + ": " + item.SerialNumber + "; ";

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
