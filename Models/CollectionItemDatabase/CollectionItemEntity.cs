using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Resources;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.UserSettings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionSetDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int CollectionItemEntityID { get; set; } // Wegen generischer Suche classname + ID verbunden
        public int? CollectionSetId { get; set; }
        public CollectionSet? CollectionSet { get; set; }

        [NotMapped]
        [Display(Name = "UniqueName", ResourceType = typeof(SharedResources))]
        public string? UniqueName { get; set; }

        [Display(Name = "Fake", ResourceType = typeof(SharedResources))]
        public bool Fake { get; set; }

        [NotMapped]
        [Display(Name = "Comment", ResourceType = typeof(SharedResources))]
        public string? Comment { get; set; }

        [Display(Name = "StatePreservationID", ResourceType = typeof(SharedResources))]
        public int? StatePreservationID { get; set; }

        [Display(Name = "StatePreservation", ResourceType = typeof(SharedResources))]
        public StatePreservation? StatePreservation { get; set; }

        [Display(Name = "CollectionItemPictureList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];

        [Display(Name = "Inscription", ResourceType = typeof(SharedResources))]
        public string? Inscription { get; set; }

        [NotMapped]
        //[Display(Name = "InscriptionTranslated", ResourceType = typeof(SharedResources))]
        public string? InscriptionTranslated { get; set; }

        [Display(Name = "CollectionItemNPartyList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNParty> CollectionItemNPartyList { get; set; } = [];

        [Display(Name = "CollectionItemNPlaceList", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];

        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int CollectionAreaID { get; set; }

        [Display(Name = "CollectionArea", ResourceType = typeof(SharedResources))]
        public CollectionArea CollectionArea { get; set; } = null!;

        [Display(Name = "ConceptValueList", ResourceType = typeof(SharedResources))]
        public List<ConceptValue> ConceptValueList { get; set; } = [];
        //public int? ObjectLayer { get; set; }
        //public ObjectLayer ObjectLayer { get; set; } = null!;

        [Display(Name = "ConceptID", ResourceType = typeof(SharedResources))]
        public int? ConceptID { get; set; }

        [Display(Name = "Concept", ResourceType = typeof(SharedResources))]
        public Concept? Concept { get; set; }

        [Display(Name = "SerialNumber", ResourceType = typeof(SharedResources))]
        [StringLength(50)]
        public string? SerialNumber { get; set; }
        [Display(Name = "CollectionItemEmbedding", ResourceType = typeof(SharedResources))]
        public CollectionItemEmbedding? CollectionItemEmbedding { get; set; }

        //Time
        [Display(Name = "ExactYear", ResourceType = typeof(SharedResources))]
        public int? ExactYear { get; set; }
        [Display(Name = "ExactYear", ResourceType = typeof(SharedResources))]
        public int? StartYear { get; set; }
        [Display(Name = "EndYear", ResourceType = typeof(SharedResources))]
        public int? EndYear { get; set; }
        [Display(Name = "IsApproximate", ResourceType = typeof(SharedResources))]
        public bool IsApproximate { get; set; }
        [NotMapped]
        [Display(Name = "Time", ResourceType = typeof(SharedResources))]
        public string Time => ExactYear != null
                    ? $"{ExactYear}"
                    : StartYear == null && EndYear == null
                        ? "Unbekannt"
                        : StartYear == null ? $"{EndYear} geschätzt" : EndYear == null ? $"{StartYear} geschätzt" : $"{StartYear} - {EndYear}";
        [Display(Name = "EraID", ResourceType = typeof(SharedResources))]
        public int? EraID { get; set; }
        [Display(Name = "Era", ResourceType = typeof(SharedResources))]
        public Era? Era { get; set; }

        //Width and Height
        [Display(Name = "Width", ResourceType = typeof(SharedResources))]
        public int? Width { get; set; }
        [Display(Name = "Width", ResourceType = typeof(SharedResources))]
        public int? Height { get; set; }
        [Display(Name = "Length", ResourceType = typeof(SharedResources))]
        public int? Length { get; set; }
        [Display(Name = "Diameter", ResourceType = typeof(SharedResources))]
        public int? Diameter { get; set; }
        [Display(Name = "Weight", ResourceType = typeof(SharedResources))]
        public int? Weight { get; set; }


        //Personal, Not for public
        [Display(Name = "PersonalIdentificationNumber", ResourceType = typeof(SharedResources))]
        public string? PersonalIdentificationNumber { get; set; }

        [Display(Name = "FilingLocation", ResourceType = typeof(SharedResources))]
        [StringLength(50)]
        public string? FilingLocation { get; set; }

        [Display(Name = "DeliveryPrice", ResourceType = typeof(SharedResources))]
        [DisplayFormat(DataFormatString = "{0:0,0.00}")]
        [Precision(18, 2)]
        public decimal? DeliveryPrice { get; set; }
        [Display(Name = "DeliveryDate", ResourceType = typeof(SharedResources))]
        public DateTime? DeliveryDate { get; set; }
        [Display(Name = "DeliveryAdress", ResourceType = typeof(SharedResources))]
        public string? DeliveryAdress { get; set; }
        [Required]
        [Display(Name = "UsingIdentityUsersID", ResourceType = typeof(SharedResources))]
        public required string UsingIdentityUsersID { get; set; }
        [Display(Name = "UsingIdentityUser", ResourceType = typeof(SharedResources))]
        public UsingIdentityUser UsingIdentityUser { get; set; } = null!;
    }
}