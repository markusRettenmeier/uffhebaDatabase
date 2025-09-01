namespace Sammlerplattform.Models.CityDatabase
{
    public interface ICitySearchParameterModel
    {
        List<int> CityID { get; set; }
        List<string> CityOeconymList_Oeconym_OeconymName { get; set; }
        List<string> CityPostalcodeList_Postalcode_PostalcodeNumber { get; set; }
        List<string> Byname { get; set; }
        List<string> Geography_GeographyName { get; set; }
        List<string> ParentCity_CityOeconymList_Oeconym_OeconymName { get; set; }
        List<int> ParentCityID { get; set; }
    }
    public class CitySearchParameterModel : ICitySearchParameterModel
    {
        public List<int> CityID { get; set; } = [];
        public List<string> CityOeconymList_Oeconym_OeconymName { get; set; } = [];
        public List<string> CityPostalcodeList_Postalcode_PostalcodeNumber { get; set; } = [];
        public List<string> Byname { get; set; } = [];
        public List<string> Geography_GeographyName { get; set; } = [];
        public List<string> ParentCity_CityOeconymList_Oeconym_OeconymName { get; set; } = [];
        public List<int> ParentCityID { get; set; } = [];
    }
}
