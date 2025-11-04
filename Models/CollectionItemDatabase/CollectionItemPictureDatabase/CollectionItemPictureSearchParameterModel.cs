namespace Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase
{
    public class CollectionItemPictureSearchParameterModel
    {
        public List<int> CollectionItemPictureID { get; set; } = [];
        public bool Frontside { get; set; }
        public List<int> CollectionItemEntityID { get; set; } = [];
    }
}
