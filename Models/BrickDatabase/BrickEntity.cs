using Sammlerplattform.Models.ProductDatabase;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickEntity : ProductEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BrickEntityID { get; set; }
        public int? BrickPotentialID { get; set; }
        public BrickPotential? BrickPotential { get; set; }
        [NotMapped]
        public ReliefType ReliefEnum
        {
            get => (ReliefType)ReliefInt; set => ReliefInt = (int)value;
        }
        [Display(Name = "Relief")]
        public int ReliefInt { get; set; }
    }

    //public enum ConditionType
    //{
    //    [Description("Keine Angabe")]
    //    NoInformation = 0,
    //    [Description("Neu")]
    //    New = 1,
    //    [Description("Gebraucht")]
    //    Used = 2,
    //    [Description("Beschädigt")]
    //    Damaged = 3,
    //    [Description("Repariert")]
    //    Repaired = 4
    //}

    public enum ReliefType
    {
        [Description("Keine Angabe")]
        NoInformation = 0,
        [Description("Vertieft")]
        Concave = 1,
        [Description("Erhaben")]
        Convex = 2
    }
}
