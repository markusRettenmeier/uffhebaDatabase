namespace Sammlerplattform.Services.ML.VectorSearch
{
    public interface IEmbeddingService
    {
        float[] GenerateEmbedding(string text);
        float[] CombineVectors(List<float[]> vectors);
    }
    // Verwenden Sie zunächst den SimpleEmbeddingService zum Testen
    public class SimpleEmbeddingService : IEmbeddingService
    {
        public float[] GenerateEmbedding(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new float[1536];

            // Text normalisieren
            text = text.ToLowerInvariant().Trim();

            // Einfaches "Bag-of-Words" ähnliches Embedding
            var embedding = new float[1536];
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Jedes Wort trägt zum Gesamtvektor bei
                var wordHash = Math.Abs(word.GetHashCode());
                var random = new Random(wordHash);

                for (int i = 0; i < embedding.Length; i++)
                {
                    embedding[i] += (float)random.NextDouble() * 2 - 1;
                }
            }

            // Normalisieren
            if (words.Length > 0)
            {
                for (int i = 0; i < embedding.Length; i++)
                {
                    embedding[i] /= words.Length;
                }
            }

            return embedding;
        }

        public float[] CombineVectors(List<float[]> vectors)
        {
            // Ihre bestehende Combine-Logik
            if (vectors == null || vectors.Count == 0)
                return new float[1536];

            var combined = new float[vectors[0].Length];
            foreach (var vector in vectors)
            {
                for (int i = 0; i < vector.Length; i++)
                {
                    combined[i] += vector[i];
                }
            }

            for (int i = 0; i < combined.Length; i++)
            {
                combined[i] /= vectors.Count;
            }

            return combined;
        }
    }


    //public class OpenAIEmbeddingService : IEmbeddingService
    //{
    //    private readonly HttpClient _httpClient;
    //    private readonly string _apiKey;

    //    public OpenAIEmbeddingService(HttpClient httpClient, IConfiguration configuration)
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
