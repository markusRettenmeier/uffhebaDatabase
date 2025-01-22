using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.PersonDatabase;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sammlerplattform.Controllers.GenericClasses;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickEntity : ProductEntity<ConditionType>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BrickEntity_ID { get; set; }
        public int? BrickPotential_ID { get; set; }
        public BrickPotential? BrickPotential { get; set; }
        public int? Brickworks_ID { get; set; }
        public Manufactory? Brickworks { get; set; }
        public int? CityOfBrickworks_ID { get; set; }
        public City? CityOfBrickworks { get; set; }
        public int? Brickmaker_ID { get; set; }
        public Person? Brickmaker { get; set; }

        [Display(Name = "Relief")]
        [NotMapped]
        public ReliefType ReliefEnum
        {
            get
            {
                return (ReliefType)ReliefInt;
            }
            set
            {
                ReliefInt = (int)value;
            }
        }
        public int ReliefInt { get; set; }

        [Display(Name = "Stichworte")]
        public int KeywordInt { get; set; }

        [Display(Name = "Stichworte")]
        [NotMapped]
        public BrickKeywordType BrickKeywordEnum
        {
            get
            {
                return (BrickKeywordType)KeywordInt;
            }
            set
            {
                KeywordInt = (int)value;
            }
        }

        [NotMapped]
        public string KeywordEnumDescription
        {
            get
            {
                Type enumType = typeof(BrickKeywordType);
                var descriptions = new List<string>();

                foreach (BrickKeywordType usage in Enum.GetValues(typeof(BrickKeywordType)))
                {
                    // Skip the 'NoInformation' flag if it is not set
                    if (usage == BrickKeywordType.NoInformation && !BrickKeywordEnum.HasFlag(BrickKeywordType.NoInformation))
                        continue;

                    if (BrickKeywordEnum.HasFlag(usage) && usage != BrickKeywordType.NoInformation)
                    {
                        // Get the member info for the enum value
                        System.Reflection.MemberInfo[] memberInfos = enumType.GetMember(usage.ToString());
                        System.Reflection.MemberInfo? enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);

                        if (enumValueMemberInfo != null)
                        {
                            // Get the Description attribute if it exists
                            var descriptionAttribute = enumValueMemberInfo
                                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                .FirstOrDefault() as DescriptionAttribute;

                            descriptions.Add(descriptionAttribute?.Description ?? usage.ToString());
                        }
                        else
                        {
                            descriptions.Add(usage.ToString());
                        }
                    }
                }

                return descriptions.Count > 0 ? string.Join(", ", descriptions) : BrickKeywordType.NoInformation.GetDescription();
            }
        }
    }

    public enum ConditionType
    {
        [Description("Keine Angabe")]
        NoInformation = 0,
        [Description("Neu")]
        New = 1,
        [Description("Gebraucht")]
        Used = 2,
        [Description("Beschädigt")]
        Damaged = 3,
        [Description("Repariert")]
        Repaired = 4
    }

    public enum ReliefType
    {
        [Description("Keine Angabe")]
        NoInformation = 0,
        [Description("Vertieft")]
        Concave = 1,
        [Description("Erhaben")]
        Convex = 2
    }

    [Flags]
    public enum BrickKeywordType
    {
        [Description("Keine Angabe")]
        NoInformation = 0,
        [Description("Abbruchziegel")]
        DemolitionBrick = 1,
        [Description("Blubl")]
        blugd = 2
    }
}
