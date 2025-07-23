using Sammlerplattform.Models.ProductPictureDatabase;

namespace Sammlerplattform.Models.Download
{
    public class PostcardDownloadModel
    {
        public List<ProductPicture> Scans { get; set; } = [];
        public YAMLPostcard Postcard { get; set; } = new();
        public YAMLArtist Artist { get; set; } = new();
        public YAMLImage Image { get; set; } = new();
        public string? Sender { get; set; }
        public YAMLReceiver Receiver { get; set; } = new();
        ////public Stamp Stamp { get; set; } = new();
        ////public Postmark SenderPostmark { get; set; } = new();
        ////public Postmark RecipientPostmark { get; set; } = new();
        public List<YAMLManufactory> Manufactorys { get; set; } = [];
    }
}
