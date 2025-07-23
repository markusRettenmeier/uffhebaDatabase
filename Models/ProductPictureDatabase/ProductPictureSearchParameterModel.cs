namespace Sammlerplattform.Models.ProductPictureDatabase
{
    public class ProductPictureSearchParameterModel
    {
        public List<int> ProductPictureID { get; set; } = [];
        public List<string> FileExtension { get; set; } = [];
        public bool Frontside { get; set; }
        public List<int> BrickEntityID { get; set; } = [];
    }
}
