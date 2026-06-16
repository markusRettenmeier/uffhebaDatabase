namespace Sammlerplattform.Models.CollectionItemDatabase.VectorSearch
{
    public class CollectionItemSearchResultDTO
    {
        public int CollectionItemEntityID { get; set; }

        public double CosineSimilarity { get; set; }

        public double SparseSimilarity { get; set; }

        public double HybridScore { get; set; }
    }
}
