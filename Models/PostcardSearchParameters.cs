using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class PostcardSearchParameters
    {
        //User
        public ICollection<string>? SearchUserName { get; set; }
        //Product
        [Range(1, 10000)]
        public ICollection<int>? SearchProduct_ID { get; set; }
        public ICollection<string>? SearchSerialNumber { get; set; }
        public ICollection<int>? SearchProductionYear { get; set; }
        //City
        public List<string>? SearchCity { get; set; }
        //Postalcode
        public ICollection<string>? SearchPostalcode { get; set; }
        //AuthorArtist
        public ICollection<string>? SearchAAName { get; set; }
        public ICollection<string>? SearchEraLong { get; set; }
        //Photography
        [RegularExpression("on")]
        public string? SearchAerial { get; set; }
        //PostcardEntity
        public ICollection<int>? SearchConditionOfCard { get; set; }
        [RegularExpression(@"4\d{0,4}")]
        public ICollection<DateTime>? SearchDateInText { get; set; }
        //PostcardImprint
        [RegularExpression("on")]
        public string? SearchFullScreen { get; set; }
        [RegularExpression("on")]
        public string? SearchPassepartout { get; set; }
        public ICollection<int>? SearchColorProcessing { get; set; }
        public ICollection<string>? SearchColorImage { get; set; }
        [RegularExpression(@"4\d{0,4}")]
        public ICollection<int>? SearchImageYear { get; set; }
        //public ICollection<string>? SearchManufacturerName { get; set; }
        public ICollection<int>? SearchManufacturers { get; set; }
        public ICollection<string>? SearchName { get; set; }
        [RegularExpression("on")]
        public string? SearchThreePictures { get; set; }
        [RegularExpression("on")]
        public string? SearchFourPictures { get; set; }
        [RegularExpression("on")]
        public string? SearchMultiPictures { get; set; }
        [RegularExpression("on")]
        public string? SearchHoax { get; set; }
        [RegularExpression("on")]
        public string? SearchGraduation { get; set; }
        [RegularExpression("on")]
        public string? SearchOccasion { get; set; }
        [RegularExpression("on")]
        public string? SearchMoon { get; set; }
        [RegularExpression("on")]
        public string? SearchStudenticaColeur { get; set; }
        public ICollection<int>? SearchImagePerception { get; set; }
        //PostcardPotential
        [RegularExpression("on")]
        public string? SearchOfficialBusiness { get; set; }
        [RegularExpression("on")]
        public string? SearchCorrugatedEdge { get; set; }
        public ICollection<string>? SearchColorRALWritingFrontside { get; set; }
        public ICollection<string>? SearchColorRALPrintingBackside { get; set; }
        [RegularExpression("on")]
        public string? SearchFieldpost { get; set; }
        public ICollection<int>? SearchFormats { get; set; }
        public ICollection<int>? SearchCardType { get; set; }
        public ICollection<int>? SearchCardSeries { get; set; }
        [RegularExpression("on")]
        public string? SearchLeporello { get; set; }
        [RegularExpression("on")]
        public string? SearchPropaganda { get; set; }
        [RegularExpression("on")]
        public string? SearchOrnament { get; set; }
        //Printing
        public ICollection<int>? SearchTechnique { get; set; }
        public ICollection<int>? SearchStyle { get; set; }
        //Stamp fehlt
        //Postmark fehlt
    }
}
