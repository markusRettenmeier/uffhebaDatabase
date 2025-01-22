using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class PostcardSearchParameters
    {
        //User
        public ICollection<string>? SearchUserName { get; set; }
        //Product
        [Range(1, 10000)]
        public ICollection<int>? SearchPostcardPotential_ID { get; set; }
        public ICollection<string>? SearchSerialNumber { get; set; }
        public ICollection<string> SearchKeywords { get; set; } = [];
        //City
        public ICollection<string>? SearchCity { get; set; }
        //Postalcode
        public ICollection<string>? SearchPostalcode { get; set; }
        //AuthorArtist
        public ICollection<string>? SearchAAName { get; set; }
        public ICollection<string>? SearchEraLong { get; set; }
        //Photography
        //PostcardEntity
        public ICollection<int>? SearchConditionOfCard { get; set; }
        [RegularExpression(@"4\d{0,4}")]
        public ICollection<DateTime>? SearchDateInText { get; set; }
        //PostcardImprint
        public ICollection<int>? SearchColorProcessing { get; set; }
        public ICollection<string>? SearchColorImage { get; set; }
        [RegularExpression(@"4\d{0,4}")]
        public ICollection<int>? SearchImageYear { get; set; }
        //public ICollection<string>? SearchManufactoryName { get; set; }
        public ICollection<int>? SearchManufactory { get; set; }
        public ICollection<string>? SearchName { get; set; }
        public ICollection<int>? SearchImagePerception { get; set; }
        //PostcardPotential
        public ICollection<string>? SearchColorRALWritingFrontside { get; set; }
        public ICollection<string>? SearchColorRALPrintingBackside { get; set; }
        public ICollection<int>? SearchFormats { get; set; }
        public ICollection<int>? SearchCardType { get; set; }
        public ICollection<int>? SearchCardSeries { get; set; }
        //Printing
        public ICollection<int>? SearchTechnique { get; set; }
        public ICollection<int>? SearchStyle { get; set; }
        //Stamp fehlt
        //Postmark fehlt
    }
}
