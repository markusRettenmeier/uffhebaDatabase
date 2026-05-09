using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase
{
    public class Industry
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [NotMapped]
        public string IndustryName { get; set; } = string.Empty;
        public List<Organization> OrganizationList { get; set; } = [];
    }
}