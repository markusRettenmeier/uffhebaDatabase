using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Services.GenericClasses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickEntity : ProductEntity<ConditionType>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int BrickEntityID { get; set; }
        public int? BrickPotentialID { get; set; }
        public BrickPotential? BrickPotential { get; set; }
        public List<BrickEntityNManufactoryNCity> BrickEntityNManufactoryNCityList { get; set; } = [];
        public List<BrickEntityNPerson> BrickEntityNPersonList { get; set; } = [];
        public List<BrickEntityNCity> BrickEntityNCityList { get; set; } = [];
        [NotMapped]
        public ReliefType ReliefEnum
        {
            get => (ReliefType)ReliefInt; set => ReliefInt = (int)value;
        }
        [Display(Name = "Relief")]
        public int ReliefInt { get; set; }

        //[Display(Name = "Stichworte")]
        //public int KeywordInt { get; set; }
        //[NotMapped]
        //public BrickKeywordType BrickKeywordEnum
        //{
        //    get => (BrickKeywordType)KeywordInt; set => KeywordInt = (int)value;
        //}

        [NotMapped]
        public string KeywordEnumDescription
        {
            get
            {
                Type enumType = typeof(BrickKeywordType);
                List<string> descriptions = [];

                foreach (BrickKeywordType usage in Enum.GetValues<BrickKeywordType>())
                {
                    // Skip the 'NoInformation' flag if it is not set
                    if (usage == BrickKeywordType.NoInformation && !BrickKeywordEnum.HasFlag(BrickKeywordType.NoInformation))
                    {
                        continue;
                    }

                    if (BrickKeywordEnum.HasFlag(usage) && usage != BrickKeywordType.NoInformation)
                    {
                        // Get the member info for the enum value
                        System.Reflection.MemberInfo[] memberInfos = enumType.GetMember(usage.ToString());
                        System.Reflection.MemberInfo? enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);

                        if (enumValueMemberInfo != null)
                        {
                            // Get the Description attribute if it exists
                            DescriptionAttribute? descriptionAttribute = enumValueMemberInfo
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
