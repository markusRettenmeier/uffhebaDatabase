namespace Sammlerplattform.Models.Translations
{
    public class TranslationDTO
    {
        public required string TextToTranslate { get; set; }
        public string? Abbreviation { get; set; }
        public required string EntityType { get; set; }
        public int EntityId { get; set; }
        public required string FieldName { get; set; }
        public required string Culture { get; set; }
        public bool IsTranslateable { get; set; } = true;
    }
}
