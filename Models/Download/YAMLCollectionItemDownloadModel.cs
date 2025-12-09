using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;

namespace Sammlerplattform.Models.Download
{
    public class YAMLCollectionItemDownloadModel
    {
        public List<CollectionItemPicture> Scans { get; set; } = [];
        public YAMLCollectionItem CollectionItem { get; set; } = new();
    }
}
