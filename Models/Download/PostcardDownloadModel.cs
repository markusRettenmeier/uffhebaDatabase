using Sammlerplattform.Models.ProductDatabase;

namespace Sammlerplattform.Models.Download
{
    public class PostcardDownloadModel
    {
        public List<ProductPicture> Scans { get; set; } = [];
        public Postcard Postcard { get; set; } = new();
        public Artist Artist { get; set; } = new();
        public Image Image { get; set; } = new();
        public string? Sender { get; set; }
        public Receiver Receiver { get; set; } = new();
        ////public Stamp Stamp { get; set; } = new();
        ////public Postmark SenderPostmark { get; set; } = new();
        ////public Postmark RecipientPostmark { get; set; } = new();
        public List<Manufactory> Manufactorys { get; set; } = [];
    }
}
