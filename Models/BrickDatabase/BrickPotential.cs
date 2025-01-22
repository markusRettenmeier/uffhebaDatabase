using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sammlerplattform.Controllers.GenericClasses;
using Sammlerplattform.Models.ProductDatabase;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickPotential : ProductPotential
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "Ziegelnummer")]
        public int BrickPotential_ID { get; set; }
        public ICollection<Brickname> BricknameSynonymICollection { get; set; } = [];
        //public int BrickPotentialGeneric_ID { get; set; }
        //[Display(Name = "Oberbegriff")]
        //public BrickPotential? BrickPotentialGeneric { get; set; }
        public ICollection<BrickPotential> BrickPotentialGeneric { get; set; } = [];
        public ICollection<BrickPotential> BrickPotentialSpeciesICollection { get; set; } = [];
        public ICollection<BrickEntity> BrickEntityICollection { get; set; } = [];

        [Display(Name = "Verwendung")]
        [NotMapped]
        public BrickUsageType UsageEnum
        {
            get
            {
                return (BrickUsageType)UsageInt;
            }
            set
            {
                UsageInt = (int)value;
            }
        }
        public int UsageInt { get; set; }
        [NotMapped]
        //public string UsageEnumDescription
        //{
        //    get
        //    {
        //        Type enumType = typeof(BrickUsageType);
        //        System.Reflection.MemberInfo[] memberInfos = enumType.GetMember(UsageEnum.ToString());
        //        System.Reflection.MemberInfo? enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
        //        if (enumValueMemberInfo != null)
        //        {
        //            var descriptionAttribute = enumValueMemberInfo
        //                .GetCustomAttributes(typeof(DescriptionAttribute), false)
        //                .FirstOrDefault() as DescriptionAttribute;
        //            return descriptionAttribute?.Description ?? UsageEnum.ToString();
        //        }
        //        else return UsageEnum.ToString();
        //    }
        //}
        public string UsageEnumDescription
        {
            get
            {
                Type enumType = typeof(BrickUsageType);
                var descriptions = new List<string>();

                foreach (BrickUsageType usage in Enum.GetValues(typeof(BrickUsageType)))
                {
                    // Skip the 'NoInformation' flag if it is not set
                    if (usage == BrickUsageType.NoInformation && !UsageEnum.HasFlag(BrickUsageType.NoInformation))
                        continue;

                    if (UsageEnum.HasFlag(usage) && usage != BrickUsageType.NoInformation)
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

                return descriptions.Count > 0 ? string.Join(", ", descriptions) : BrickUsageType.NoInformation.GetDescription();
            }
        }

        public string? Description { get; set; }
    }

    [Flags]
    public enum BrickUsageType
    {
        [Description("Keine Angabe")]
        NoInformation = 0,
        [Description("Mauer")]
        Wall = 1 << 0,  // 1
        [Description("Dach")]
        Roof = 1 << 1,  // 2
        [Description("Decke")]
        Ceiling = 1 << 2,  // 4
        [Description("Fußboden")]
        Floor = 1 << 3,  // 8
    }
}