using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using Sammlerplattform.Services.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_CollectionItemId_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_CollectionItemId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }
        [Required(ErrorMessageResourceName = "Error_CollectionAreaId_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_CollectionAreaId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int CollectionAreaId { get; set; }
        [Display(Name = "UniqueName", ResourceType = typeof(SharedResources))]
        public string? UniqueName { get; set; }
        [Display(Name = "Fake", ResourceType = typeof(SharedResources))]
        public bool Fake { get; set; }
        [Display(Name = "Comment", ResourceType = typeof(SharedResources))]
        public string? Comment { get; set; }
        [Display(Name = "StatePreservation", ResourceType = typeof(SharedResources))]
        public int? StatePreservationID { get; set; }
        [Display(Name = "Inscription", ResourceType = typeof(SharedResources))]
        public string? Inscription { get; set; }
        [Display(Name = "InscriptionTranslated", ResourceType = typeof(SharedResources))]
        public string? InscriptionTranslated { get; set; }
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

        [Display(Name = "EraID", ResourceType = typeof(SharedResources))]
        public int? EraID { get; set; }
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
        [ModelBinder(BinderType = typeof(CultureAwareDecimalBinder))]
        public decimal? DeliveryPrice { get; set; }

        [Display(Name = "DeliveryDate", ResourceType = typeof(SharedResources))]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "DeliveryAdress", ResourceType = typeof(SharedResources))]
        public string? DeliveryAdress { get; set; }

        [Display(Name = "IsPublic", ResourceType = typeof(SharedResources))]
        public bool IsCollectionItemPublic { get; set; }
        public List<PictureToCollectionItemEditDTO> CollectionItemPictureList { get; set; } = [];
        public string? DeletedPictureIds { get; set; }
        public List<ConceptValueToCollectionItemEditDTO> ConceptValueList { get; set; } = [];
        //public List<OwnershipProofPictureToCollectionItemEditDTO> OwnershipProofPictureList { get; set; } = [];
        public List<ParticipantToCollectionItemCreateDTO> ConnectedParticipantList { get; set; } = [];
        public List<PlaceToCollectionItemCreateDTO> ConnectedPlaceList { get; set; } = [];
    }
    public class PictureToCollectionItemEditDTO
    {
        public int? Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessageResourceName = "Error_Perspective_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int PerspectiveInt { get; set; }
        public PerspectiveType Perspective
        {
            get => (PerspectiveType)PerspectiveInt; set => PerspectiveInt = (int)value;
        }
        public string PerspectiveDisplay => Perspective.GetDisplayName();
        public IFormFile? IFormFile { get; set; }
        public bool Frontside => PerspectiveInt == 0;
    }
    public class ConceptValueToCollectionItemEditDTO
    {
        [Required(ErrorMessageResourceName = "Error_ConceptId_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_ConceptId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int ConceptId { get; set; }
        public int ConceptValueId { get; set; }
        public string? ValueString { get; set; }
        public int? ValueInt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0,0.00}")]
        public decimal? ValueDecimal { get; set; }
        public DateTime? ValueDate { get; set; }
        public bool? ValueBool { get; set; }
    }
    //public class OwnershipProofPictureToCollectionItemEditDTO
    //{
    //    public int? Id { get; set; }

    //    [Display(Name = "IFormFile", ResourceType = typeof(SharedResources))]
    //    public IFormFile? FormFile { get; set; }
    //    public OwnershipProofPictureType Type { get; set; }
    //}
}
