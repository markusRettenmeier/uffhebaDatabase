using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;

namespace Sammlerplattform.Models.Download
{
    public class YAMLCollectionItemDownloadModel
    {
        public List<CollectionItemPicture> Scans { get; set; } = [];
        public YAMLCollectionItem Product { get; set; } = new();
    }
}
