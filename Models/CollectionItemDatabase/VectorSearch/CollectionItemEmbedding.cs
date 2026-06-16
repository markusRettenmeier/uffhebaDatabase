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
        public string DenseEmbeddingJson { get; set; } = string.Empty;

        [NotMapped]
        public float[] DenseEmbedding
        {
            get => JsonSerializer.Deserialize<float[]>(DenseEmbeddingJson) ?? [];
            set => DenseEmbeddingJson = JsonSerializer.Serialize(value);
        }

        // Für SQL Server: Vektor als JSON oder binary speichern
        [Column(TypeName = "nvarchar(max)")] // Oder varbinary(max) für kompakte Speicherung
        public string SparseWeightsJson { get; set; } = string.Empty;

        [NotMapped]
        public Dictionary<int, float> HandleSparseWeigths
        {
            get => JsonSerializer.Deserialize<Dictionary<int, float>>(SparseWeightsJson) ?? [];
            set => SparseWeightsJson = JsonSerializer.Serialize(value);
        }

        public int CollectionItemEntityID { get; set; }
        public CollectionItemEntity CollectionItemEntity { get; set; } = null!;

        public DateTime LastUpdated { get; set; }
    }
}
