using Sammlerplattform.Models.ProductPictureDatabase;

namespace Sammlerplattform.Models.Download
{
    public class YAMLBrickDownloadModel
    {
        public List<ProductPicture> Scans { get; set; } = [];
        public YAMLBrick Brick { get; set; } = new();
        public List<YAMLManufactory> Manufactorys { get; set; } = [];
        public List<YAMLPerson> People { get; set; } = [];
    }
}
