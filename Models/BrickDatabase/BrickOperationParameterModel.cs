using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickOperationParameterModel
    {
        public BrickPotential BrickPotential { get; set; } = new();
        public BrickEntity BrickEntity { get; set; } = new() { UsingIdentityUsersID = string.Empty };
        public Person Person { get; set; } = new() { Name = string.Empty };
        public List<ProductPicture> ProductPictureList { get; set; } = [];
        public Brickname Brickname { get; set; } = new() { Name = string.Empty };
        [Display(Name = "Personen")]
        public List<BrickEntityNPerson> BrickEntityNPersonList { get; set; } = [];
        [Display(Name = "Orte")]
        public List<BrickEntityNCity> BrickEntityNCityList { get; set; } = [];
        public List<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityList { get; set; } = [];
        public List<(Manufactory manufactory, int? selectedCityID, List<City> cityList)> ManufactoryTupleList { get; set; } = [];
        [Display(Name = "Hersteller")]
        public List<ManufactoryCityView> ManufactoryNCityList { get; set; } = [];
        public Era Era { get; set; } = new() { EraName = string.Empty };
        [Display(Name = "Herstellungsverfahren")]
        public ProcessOfManufacture ProcessOfManufacture { get; set; } = new() { ProcessOfManufactureName = string.Empty, Mainprocess = string.Empty };
        public List<ProductNColorVariant> ProductNColorVariantList { get; set; } = [];
        [Display(Name = "Farben")]
        public List<Color> ColorList { get; set; } = [];
        public List<ProductNMaterial> ProductNMaterialList { get; set; } = [];
        [Display(Name = "Materialien")]
        public List<Material> MaterialList { get; set; } = [];
        public List<ProductNKeyword> ProductNKeywordList { get; set; } = [];
        [Display(Name = "Stichworte")]
        public List<Keyword> KeywordList { get; set; } = [];
        public List<Condition> ConditionList { get; set; } = [];
    }

    public class ManufactoryCityView
    {
        public string ManufactoryName { get; set; } = string.Empty;
        public string? City { get; set; }
    }
}