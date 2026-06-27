using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase.IndustryDatabase;

public class Industry
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; set; }
    public List<Organization> OrganizationList { get; set; } = [];
}