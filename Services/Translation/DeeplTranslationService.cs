using DeepL;
using System.Globalization;

namespace Sammlerplattform.Services.Translation
{
    public interface IDeeplTranslationService
    {
        Task<Dictionary<string, string>> TranslateTextDifferentCulturesAsync(string text, string? exceptOfCulture);
        string SetIntoFallbackLanguage(string text);
        string NetCultureToDeeplLanguage(string netCulture);
    }

    public class DeeplTranslationService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IDeeplTranslationService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly string _apiKey = configuration["DeepL:ApiKey"] ?? throw new Exception("DeepL API key not configured");

        public async Task<Dictionary<string, string>> TranslateTextDifferentCulturesAsync(string text, string? exceptOfCulture)
        {
            var client = new DeepLClient(_apiKey);
            Dictionary<string, string> translations = [];
            string[] supportedCultures = { LanguageCode.German };
            foreach (var targetLang in supportedCultures.Where(x => !string.IsNullOrEmpty(exceptOfCulture) && !x.Equals(exceptOfCulture)))
            {
                var result = await client.TranslateTextAsync(text, null, LanguageCode.German);
                translations[targetLang.ToString()] = result.Text;
            }
            return translations;
        }

        public string SetIntoFallbackLanguage(string text)
        {
            string textEnglish = text;
            if (CultureInfo.CurrentCulture.Name != "en-US")
            {
                // Ensure the collectionAreaName is in English for consistency
                textEnglish = TranslateFallbackEnglish(text).Result;
            }

            return textEnglish;
        }
        private async Task<string> TranslateFallbackEnglish(string text)
        {
            var client = new DeepLClient(_apiKey);
            var result = await client.TranslateTextAsync(text, null, LanguageCode.EnglishAmerican);

            return result.Text;
        }
        public string NetCultureToDeeplLanguage(string netCulture)
        {
            return netCulture switch
            {
                "de" or "de-DE" => LanguageCode.German,
                "en" or "en-US" => LanguageCode.EnglishAmerican,
                _ => LanguageCode.EnglishAmerican.ToString(),
            };
        }

        private class DeepLResponse
        {
            public Translation[]? Translations { get; set; }
        }

        private class Translation
        {
            public string? Text { get; set; }
        }
    }

}
