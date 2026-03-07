using Sammlerplattform.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.CollectionItemDatabase.OwnershipProofPictureDatabase
{
    public class OwnershipProofPicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OwnershipProofPictureID { get; set; }
        public int OwnershipProofPictureTypeInt { get; set; }
        [NotMapped]
        public OwnershipProofPictureType OwnershipProofPictureType
        {
            get => (OwnershipProofPictureType)OwnershipProofPictureTypeInt;
            set => OwnershipProofPictureTypeInt = (int)value;
        }
        public int CollectionItemEntityID { get; set; }
        public CollectionItemEntity CollectionItemEntity { get; set; } = new() { UsingIdentityUsersID = string.Empty };
        [NotMapped]
        [Display(Name = "IFormFile", ResourceType = typeof(SharedResources))]
        public IFormFile? IFormFile { get; set; }
        /// <summary>
        /// Folgende drei werden ersetzt, weenn Zeitreihendatenbank und HSM (Hardware Security Module) implementiert sind.
        /// </summary>
        public byte[] Signature { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    public enum OwnershipProofPictureType
    {
        [Display(Name = "OwnershipProof_Type_BillOfSale", ResourceType = typeof(SharedResources))]
        Invoice = 0,
        [Display(Name = "OwnershipProof_Type_Certificate", ResourceType = typeof(SharedResources))]
        Certificate = 1,
        [Display(Name = "OwnershipProof_Type_Other", ResourceType = typeof(SharedResources))]
        Other = 2
    }
}
