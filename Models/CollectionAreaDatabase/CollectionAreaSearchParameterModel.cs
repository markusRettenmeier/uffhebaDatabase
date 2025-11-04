namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionAreaSearchParameterModel
    {
        public List<int> CollectionAreaID { get; set; } = [];
        public List<string> CollectionAreaName { get; set; } = [];
        public List<int> CollectionAttribute_CollectionAttributeID { get; set; } = [];
        public List<string> CollectionAttribute_CollectionAttributeName { get; set; } = [];
        public List<int> CollectionAttribute_CollectionAttributeTypeInt { get; set; } = [];
    }
}
