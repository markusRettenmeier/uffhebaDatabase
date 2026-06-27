using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.ConceptualRelationshipDatabase.ConceptValueDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemDisplayDTO
    {
        public int CollectionItemEntityID { get; set; } // Wegen generischer Suche classname + ConceptId verbunden

        [Display(Name = "UniqueName", ResourceType = typeof(SharedResources))]
        public string? UniqueName { get; set; }

        [Display(Name = "Fake", ResourceType = typeof(SharedResources))]
        public bool Fake { get; set; }

        [Display(Name = "Comment", ResourceType = typeof(SharedResources))]
        public string? Comment { get; set; }

        [Display(Name = "StatePreservationID", ResourceType = typeof(SharedResources))]
        public int? StatePreservationID { get; set; }
        public string? StatePreservationName { get; set; }

        [Display(Name = "Inscription", ResourceType = typeof(SharedResources))]
        public string? Inscription { get; set; }
        public string? InscriptionTranslated { get; set; }

        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        public int CollectionAreaID { get; set; }

        [Display(Name = "SerialNumber", ResourceType = typeof(SharedResources))]
        public string? SerialNumber { get; set; }

        //Time
        [Display(Name = "ExactYear", ResourceType = typeof(SharedResources))]
        public int? ExactYear { get; set; }

        [Display(Name = "StartYear", ResourceType = typeof(SharedResources))]
        public int? StartYear { get; set; }

        [Display(Name = "EndYear", ResourceType = typeof(SharedResources))]
        public int? EndYear { get; set; }

        [Display(Name = "IsApproximate", ResourceType = typeof(SharedResources))]
        public bool IsApproximate { get; set; }

        [Display(Name = "Time", ResourceType = typeof(SharedResources))]
        public string Time => ExactYear != null
                    ? $"{ExactYear}"
                    : StartYear == null && EndYear == null
                        ? string.Empty
                        : StartYear == null ? $"{EndYear} geschätzt" : EndYear == null ? $"{StartYear} geschätzt" : $"{StartYear} - {EndYear}";
        [Display(Name = "EraID", ResourceType = typeof(SharedResources))]
        public int? EraID { get; set; }
        [Display(Name = "Era", ResourceType = typeof(SharedResources))]
        public string? EraName { get; set; }

        //Width and Height
        [Display(Name = "Width", ResourceType = typeof(SharedResources))]
        public int? Width { get; set; }

        [Display(Name = "Height", ResourceType = typeof(SharedResources))]
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
        public string? FilingLocation { get; set; }

        [Display(Name = "DeliveryPrice", ResourceType = typeof(SharedResources))]
        public decimal? DeliveryPrice { get; set; }

        [Display(Name = "DeliveryDate", ResourceType = typeof(SharedResources))]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "DeliveryAdress", ResourceType = typeof(SharedResources))]
        public string? DeliveryAdress { get; set; }

        [Display(Name = "IsPublic", ResourceType = typeof(SharedResources))]
        public bool IsCollectionItemPublic { get; set; } = true;

        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];

        [Display(Name= "CollectionArea", ResourceType = typeof(SharedResources))]
        public string CollectionAreaName { get; set; } = string.Empty;

        [Display(Name = "Participants", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNParticipantDisplayDTO> CollectionItemNParticipantList { get; set; } = [];

        [Display(Name = "ConnectedPlaces", ResourceType = typeof(SharedResources))]
        public List<CollectionItemNPlaceDisplayDTO> CollectionItemNPlaceList { get; set; } = [];
        public List<ConceptValue> ConceptValueList { get; set; } = [];

        [Display(Name = "DisplayName", ResourceType = typeof(SharedResources))]
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(UniqueName))
                    return UniqueName;
                else if (!string.IsNullOrEmpty(PersonalIdentificationNumber))
                    return PersonalIdentificationNumber;
                else
                    return CollectionItemEntityID.ToString();
            }
        }
    }
    public class CollectionItemNParticipantDisplayDTO : ParticipantDisplayDTO
    {
        public int RelationshipId { get; set; }
        public string RelationshipName { get; set; } = string.Empty;
    }
    public class CollectionItemNPlaceDisplayDTO : PlaceDisplayDTO
    {
        public int RelationshipId { get; set; }
        public string RelationshipName { get; set; } = string.Empty;
    }
}
