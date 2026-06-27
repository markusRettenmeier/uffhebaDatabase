using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.Translations
{
    public class EntityTranslation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int EntityTranslationID { get; set; }

        public required string TranslatedText { get; set; }
        public string? Abbreviation { get; set; }

        // Composite Index für Performance
        [Index("IX_EntityTranslations_Lookup", IsUnique = true, Order = 1)]
        public required string EntityName { get; set; } //Tabellenname

        [Index("IX_EntityTranslations_Lookup", IsUnique = true, Order = 2)]
        public int EntityId { get; set; }

        [Index("IX_EntityTranslations_Lookup", IsUnique = true, Order = 3)]
        public required string PropertyName { get; set; }

        [Index("IX_EntityTranslations_Lookup", IsUnique = true, Order = 4)]
        public required string Culture { get; set; }
    }
}
