using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductDatabase;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickSearchParameterModel : IProductSearchParameterModel, ICitySearchParameterModel, IManufactorySearchParameter, IPersonSearchParameterModel
    {
        // Potential
        public ICollection<int> SearchBrickPotential_ID { get; set; } = [];
        public ICollection<string> SearchBrickname { get; set; } = [];
        public ICollection<string> SearchRelief { get; set; } = [];
        public ICollection<string> SearchUsage { get; set; } = [];
        public ICollection<int> SearchBrickEntity_ID { get; set; } = [];

        // Inherited
        // Potential
        public ICollection<string> SearchSerialnumber { get; set; } = [];
        // Entity
        public ICollection<string> SearchFilingLocation { get; set; } = [];
        public ICollection<decimal> SearchPrice { get; set; } = [];
        public string? SearchFake { get; set; }
        public ICollection<string> SearchMaterial { get; set; } = [];
        ICollection<string> IProductSearchParameterModel.SearchUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<string> SearchCondition { get; set; } = [];
        public ICollection<int> SearchWidth { get; set; } = [];
        public ICollection<int> SearchHeight { get; set; } = [];
        public ICollection<int> SearchLength { get; set; } = [];
        ICollection<int> IProductSearchParameterModel.SearchProductionSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<int> ICitySearchParameterModel.SearchCity_ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<string> ICitySearchParameterModel.SearchOeconym { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<string> ICitySearchParameterModel.SearchPostalcode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<string> ICitySearchParameterModel.SearchByname { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<string> ICitySearchParameterModel.SearchGeography { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<string> ICitySearchParameterModel.SearchParentCity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<int> ICitySearchParameterModel.SearchParentCity_ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<string> SearchManufactory { get; set; } = [];
        ICollection<string> IManufactorySearchParameter.SearchOeconym { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<string> IManufactorySearchParameter.SearchProductionFacility { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<int> SearchYear { get; set; } = [];
        ICollection<int> IPersonSearchParameterModel.SearchPersonID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<string> SearchSignature { get; set; } = [];
        public ICollection<string> SearchName { get; set; } = [];
        public ICollection<string> SearchPseudonym { get; set; } = [];
        public ICollection<int> SearchBirthYear { get; set; } = [];
        public ICollection<int> SearchDeathYear { get; set; } = [];
    }
}
