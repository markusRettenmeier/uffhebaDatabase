namespace Sammlerplattform.Models.Translations
{
    public class EntityTranslationSearchParameter
    {
        public List<string> EntityName { get; set; } = [];
        public List<int> EntityId { get; set; } = [];
        public List<string> PropertyName { get; set; } = [];
        public List<string> Culture { get; set; } = [];
        public List<string> TranslatedText { get; set; } = [];
    }
}
