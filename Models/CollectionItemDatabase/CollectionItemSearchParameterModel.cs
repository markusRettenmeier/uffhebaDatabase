namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemSearchParameterModel
    {
        public List<int> CollectionItemEntityID { get; set; } = [];
        public List<int> CollectionAreaID { get; set; } = [];
        public List<string> UniqueName { get; set; } = [];
        public List<string> UsingIdentityUsersID { get; set; } = [];

        // Neue Properties für Embedding-Suche
        public string? SemanticSearchQuery { get; set; }
    }
}
