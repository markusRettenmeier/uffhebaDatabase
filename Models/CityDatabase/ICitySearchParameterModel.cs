namespace Sammlerplattform.Models.CityDatabase
{
    public interface ICitySearchParameterModel
    {
        List<int> CityID { get; set; }
        List<string> CityNOeconymList_Oeconym_OeconymName { get; set; }
        List<string> PostalcodeList_PostalcodeNumber { get; set; }
        List<string> Byname { get; set; }
        List<string> Geography_GeographyName { get; set; }
        List<string> ParentCity_CityNOeconymList_Oeconym_OeconymName { get; set; }
        List<int> ParentCityID { get; set; }
    }
    public class CitySearchParameterModel : ICitySearchParameterModel
    {
        public List<int> CityID { get; set; } = [];
        public List<string> CityNOeconymList_Oeconym_OeconymName { get; set; } = [];
        public List<string> PostalcodeList_PostalcodeNumber { get; set; } = [];
        public List<string> Byname { get; set; } = [];
        public List<string> Geography_GeographyName { get; set; } = [];
        public List<string> ParentCity_CityNOeconymList_Oeconym_OeconymName { get; set; } = [];
        public List<int> ParentCityID { get; set; } = [];
    }
}
