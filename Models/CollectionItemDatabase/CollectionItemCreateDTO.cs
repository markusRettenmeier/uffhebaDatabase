using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Resources;
using Sammlerplattform.Services;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.CollectionItemDatabase
{
    public class CollectionItemCreateDTO
    {
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

        [Display(Name = "SerialNumber", ResourceType = typeof(SharedResources))]
        public string? SerialNumber { get; set; }

        [Display(Name = "CollectionAreaID", ResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_CollectionAreaId_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int CollectionAreaID { get; set; }

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
        public bool IsCollectionItemPublic { get; set; } = true;

        public List<PictureToCollectionItemCreateDTO> CollectionItemPictureList { get; set; } = [];
        public List<ConceptValueToCollectionItemCreateDTO> ConceptValueList { get; set; } = [];
        //public List<OwnershipProofPictureToCollectionItemCreateDTO> OwnershipProofPictureList { get; set; } = [];
        public List<ParticipantToCollectionItemCreateDTO> ParticipantctionItemList { get; set; } = [];
        public List<PlaceToCollectionItemCreateDTO> PlaceToCollectionItemList { get; set; } = [];
    }

    public class PictureToCollectionItemCreateDTO
    {
        [Display(Name = "Perspective", ResourceType = typeof(SharedResources))]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "Error_Perspective_Range", ErrorMessageResourceType = typeof(SharedResources))]
        public int PerspectiveInt { get; set; }

        [Required(ErrorMessageResourceName = "Error_CollectionItemPicture_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = "IFormFile", ResourceType = typeof(SharedResources))]
        public IFormFile? IFormFile { get; set; }

        public bool Frontside => PerspectiveInt == 0;
    }
    public class StatePreservationViewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ConceptValueToCollectionItemCreateDTO
    {
        [Display(Name = "ConceptID", ResourceType = typeof(SharedResources))]
        [Required(ErrorMessageResourceName = "Error_ConceptID_Required", ErrorMessageResourceType = typeof(SharedResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Error_Id_Missing", ErrorMessageResourceType = typeof(SharedResources))]
        public int ConceptId { get; set; }
        public string? ValueString { get; set; }
        public int? ValueInt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0,0.00}")]
        public decimal? ValueDecimal { get; set; }
        public DateTime? ValueDate { get; set; }
        public bool ValueBool { get; set; }
    }
    //public class OwnershipProofPictureToCollectionItemCreateDTO
    //{
    //    [Required(ErrorMessageResourceName = "Error_OwnershipProofPicture_IsMissing", ErrorMessageResourceType = typeof(SharedResources))]
    //    [Display(Name = "IFormFile", ResourceType = typeof(SharedResources))]
    //    public IFormFile FormFile { get; set; } = null!;
    //    public OwnershipProofPictureType Type { get; set; }
    //}
    public class ParticipantToCollectionItemCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_ParticipantID_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_Relationship_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Relationship { get; set; }
    }
    public class PlaceToCollectionItemCreateDTO
    {
        [Required(ErrorMessageResourceName = "Error_PlaceID_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Error_Relationship_Required", ErrorMessageResourceType = typeof(SharedResources))]
        public required string Relationship { get; set; }
    }
}
