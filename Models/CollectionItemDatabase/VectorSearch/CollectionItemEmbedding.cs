using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Sammlerplattform.Models.CollectionItemDatabase.VectorSearch
{
    public class CollectionItemEmbedding
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CollectionItemEmbeddingID { get; set; }

        public int CollectionItemEntityID { get; set; }
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;

        // Für SQL Server: Vektor als JSON oder binary speichern
        [Column(TypeName = "nvarchar(max)")] // Oder varbinary(max) für kompakte Speicherung
        public string CombinedEmbeddingJson { get; set; } = string.Empty;

        [NotMapped]
        public float[] CombinedEmbedding
        {
            get => JsonSerializer.Deserialize<float[]>(CombinedEmbeddingJson) ?? [];
            set => CombinedEmbeddingJson = JsonSerializer.Serialize(value);
        }

        public DateTime LastUpdated { get; set; }
    }
}
