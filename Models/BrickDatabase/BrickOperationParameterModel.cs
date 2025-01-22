using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickOperationParameterModel
    {
        public BrickPotential BrickPotential { get; set; } = new();
        public BrickEntity BrickEntity { get; set; } = new();
        public (Manufactory manufactory, int? selectedCityID, List<City> cityList) BrickworksTuple { get; set; } = new();
        public ManufacturingDate ManufacturingDate { get; set; } = new();
        public Person Manufacturer { get; set; } = new() { Name = string.Empty };
        public ProductPicture ProductPicture { get; set; } = new();
        public Brickname Brickname { get; set; } = new() { Name = string.Empty };
        public List<string> TextPositionString { get; set; } = [];
    }
}