using DeepL;
using System.Globalization;

namespace Sammlerplattform.Services.Translation
{
    public interface IDeeplTranslationService
    {
        Task<List<(string culture, string translatedText, string? abbreviation)>> TranslateTextDifferentCulturesAsync(string textn, string? exceptOfCulture);
        string NetCultureToDeeplLanguage(string netCulture);
    }

    public class DeeplTranslationService(IConfiguration configuration, ITrackEvents trackEvents) : IDeeplTranslationService
    {
        private readonly string _apiKey = configuration["DeepL:ApiKey"] ?? throw new Exception("DeepL API key not configured");

        public async Task<List<(string culture, string translatedText, string? abbreviation)>> TranslateTextDifferentCulturesAsync(string text, string? exceptOfCulture)
        {
            var client = new DeepLClient(_apiKey);
            List<(string culture, string translatedText, string? abbreviation)> translations = [];
            string[] supportedCultures = [LanguageCode.German, LanguageCode.EnglishAmerican, LanguageCode.French, LanguageCode.Spanish];
            foreach (var targetLang in supportedCultures.Where(x => !string.IsNullOrEmpty(exceptOfCulture) && !x.Equals(exceptOfCulture)))
            {
                var result = await client.TranslateTextAsync(text, null, LanguageCode.German);
                translations.Add((targetLang, result.Text, null)); // Abbreviations are not delivered by DeepL
            }
            return translations;
        }
        private async Task<string> TranslateFallbackEnglish(string text)
        {
            var client = new DeepLClient(_apiKey);
            var result = await client.TranslateTextAsync(text, null, LanguageCode.EnglishAmerican);

            return result.Text;
        }
        public string NetCultureToDeeplLanguage(string netCulture)
        {
            trackEvents.TrackInfo("DeeplTranslationService/NetCultureToDeeplLanguage: Converting .NET culture to DeepL language code.", new Dictionary<string, object>
            {
                { "NetCulture", netCulture }
            });

            return netCulture switch
            {
                "de" or "de-DE" => LanguageCode.German,
                "en" or "en-US" => LanguageCode.EnglishAmerican,
                "fr" or "fr-FR" => LanguageCode.French,
                "es" or "es-ES" => LanguageCode.Spanish,
                _ => LanguageCode.EnglishAmerican.ToString()
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
