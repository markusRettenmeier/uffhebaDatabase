using System.Text.Json.Serialization;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessOpenAIEmbedding
    {
        //Task<float[]> GenerateEmbeddingAsync(string text);
        //float[] CombineVectorsAsync(List<float[]> vectors);
    }
    //public class OpenAIEmbeddingProcessor : IProcessOpenAIEmbedding
    //{
    //    private readonly HttpClient _httpClient;
    //    private readonly string _apiKey;

    //    public OpenAIEmbeddingProcessor(HttpClient httpClient, IConfiguration configuration)
    //    {
    //        _httpClient = httpClient;
    //        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API Key is missing");

    //        _httpClient.DefaultRequestHeaders.Authorization =
    //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
    //    }

    //    public async Task<float[]> GenerateEmbeddingAsync(string text)
    //    {
    //        if (string.IsNullOrEmpty(text))
    //            return new float[1536]; // Standard-Größe für text-embedding-3-small

    //        try
    //        {
    //            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/embeddings", new
    //            {
    //                input = text,
    //                model = "text-embedding-3-small"
    //            });

    //            if (!response.IsSuccessStatusCode)
    //            {
    //                var errorContent = await response.Content.ReadAsStringAsync();
    //                throw new HttpRequestException($"OpenAI API error: {response.StatusCode} - {errorContent}");
    //            }

    //            var result = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>();

    //            if (result?.Data?.FirstOrDefault()?.Embedding == null)
    //            {
    //                throw new InvalidOperationException("No embedding data received from OpenAI");
    //            }

    //            return result.Data[0].Embedding;
    //        }
    //        catch (Exception ex)
    //        {
    //            // Logging hinzufügen
    //            Console.WriteLine($"Error generating embedding: {ex.Message}");
    //            throw;
    //        }
    //    }

    //    public float[] CombineVectorsAsync(List<float[]> vectors)
    //    {
    //        if (vectors == null || vectors.Count == 0)
    //            return new float[1536];

    //        // Einfache Durchschnittsbildung
    //        int dimension = vectors[0].Length;
    //        float[] combinedVector = new float[dimension];
    //        foreach (var vector in vectors)
    //        {
    //            for (int i = 0; i < dimension; i++)
    //            {
    //                combinedVector[i] += vector[i];
    //            }
    //        }

    //        for (int i = 0; i < dimension; i++)
    //        {
    //            combinedVector[i] /= vectors.Count;
    //        }

    //        return combinedVector;
    //    }

    //    public class OpenAIEmbeddingResponse
    //    {
    //        [JsonPropertyName("object")]
    //        public string Object { get; set; } = string.Empty;

    //        [JsonPropertyName("data")]
    //        public List<EmbeddingData> Data { get; set; } = new();

    //        [JsonPropertyName("model")]
    //        public string Model { get; set; } = string.Empty;

    //        [JsonPropertyName("usage")]
    //        public UsageInfo Usage { get; set; } = new();
    //    }

    //    public class EmbeddingData
    //    {
    //        [JsonPropertyName("object")]
    //        public string Object { get; set; } = string.Empty;

    //        [JsonPropertyName("index")]
    //        public int Index { get; set; }

    //        [JsonPropertyName("embedding")]
    //        public float[] Embedding { get; set; } = Array.Empty<float>();
    //    }

    //    public class UsageInfo
    //    {
    //        [JsonPropertyName("prompt_tokens")]
    //        public int PromptTokens { get; set; }

    //        [JsonPropertyName("total_tokens")]
    //        public int TotalTokens { get; set; }
    //    }
    //}
}
