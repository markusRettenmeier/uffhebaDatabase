namespace Sammlerplattform.Models.ProductPictureDatabase
{
    public class ProductPictureSearchParameterModel
    {
        public ICollection<int> SearchProductPicture_ID { get; set; } = [];
        public ICollection<string> SearchFileExtension { get; set; } = [];
        public ICollection<string> SearchSide { get; set; } = [];
    }
}
