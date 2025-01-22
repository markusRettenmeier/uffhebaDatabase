using Sammlerplattform.Models.BrickDatabase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ProductPictureDatabase
{
    public class ProductPicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductPicture_ID { get; set; }
        public string? FileExtension { get; set; }
        public bool Frontside { get; set; }
        [Display(Name = "Perspektive")]
        [NotMapped]
        public PerspectiveType Perspective
        {
            get
            {
                return PerspectiveInt == null ? PerspectiveType.NoInformation : (PerspectiveType)PerspectiveInt;
            }
            set
            {
                PerspectiveInt = (int)value;
            }
        }
        public int? PerspectiveInt { get; set; }
        public int? PostcardEntity_ID { get; set; }
        public PostcardEntity? PostcardEntity { get; set; }
        public int? BrickEntity_ID { get; set; }
        public BrickEntity? BrickEntity { get; set; }
        public string? TextPositionJson { get; set; }
    }
    public enum PerspectiveType
    {
        NoInformation = 0,
        Frontside = 1,
        Backside = 2
    } 
}