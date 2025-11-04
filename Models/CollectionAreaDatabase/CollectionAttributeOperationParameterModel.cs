namespace Sammlerplattform.Models.CollectionAreaDatabase
{
    public class CollectionAttributeOperationParameterModel
    {
        public CollectionArea CollectionArea { get; set; } = new() { CollectionAreaName = string.Empty };
        public CollectionAttribute CollectionAttribute { get; set; } = new() { CollectionAttributeName = string.Empty };
    }
}
