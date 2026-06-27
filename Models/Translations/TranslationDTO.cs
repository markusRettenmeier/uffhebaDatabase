namespace Sammlerplattform.Models.Translations
{
    public class TranslationDTO
    {
        public required string TextToTranslate { get; set; }
        public string? Abbreviation { get; set; }
        public required string EntityName { get; set; }
        public int EntityId { get; set; }
        public required string PropertyName { get; set; }
        public bool IsTranslateable { get; set; } = true;
    }
}
