using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.StatePreservationDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.VectorSearch;
using Sammlerplattform.Models.ConceptualRelationshipDatabase.ConceptValueDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "CollectionItemEntityID", ResourceType = typeof(SharedResources))]
        public int CollectionItemEntityID { get; set; } // Wegen generischer Suche classname + ConceptId verbunden
        public bool Fake { get; set; }
        public int? StatePreservationID { get; set; }
        public StatePreservation? StatePreservation { get; set; }
        public List<CollectionItemPicture> CollectionItemPictureList { get; set; } = [];
        public string? Inscription { get; set; }
        public List<CollectionItemNParticipant> CollectionItemNParticipantList { get; set; } = [];
        public List<CollectionItemNPlace> CollectionItemNPlaceList { get; set; } = [];
        public int CollectionAreaID { get; set; }
        public CollectionArea CollectionArea { get; set; } = null!;
        public List<ConceptValue> ConceptValueList { get; set; } = [];
        public string? SerialNumber { get; set; }
        public CollectionItemEmbedding? CollectionItemEmbedding { get; set; }
        public int? ExactYear { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsApproximate { get; set; }
        public int? EraID { get; set; }
        public Era? Era { get; set; }

        //Width and Height
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Length { get; set; }
        public int? Diameter { get; set; }
        public int? Weight { get; set; }


        //Personal, Not for public
        public string? PersonalIdentificationNumber { get; set; }
        public string? FilingLocation { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveryAdress { get; set; }
        public required string UsingIdentityUsersID { get; set; }
        public UsingIdentityUser UsingIdentityUser { get; set; } = null!;
        public bool IsCollectionItemPublic { get; set; } = true;

        //public List<OwnershipProofPictureDatabase.OwnershipProofPicture> OwnershipProofPictureList { get; set; } = [];
    }
}