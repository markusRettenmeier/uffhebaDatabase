using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Services;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sammlerplattform.Models.BrickDatabase
{
    public class BrickPotential : ProductPotential
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Display(Name = "Ziegelnummer")]
        public int? BrickPotentialID { get; set; }
        public List<Brickname> BricknameSynonymList { get; set; } = [];
        public List<BrickEntity> BrickEntityList { get; set; } = [];
        public int UsageInt { get; set; }

        [Display(Name = "Verwendung")]
        [NotMapped]
        public BrickUsageType UsageEnum
        {
            get => (BrickUsageType)UsageInt; set => UsageInt = (int)value;
        }
        [NotMapped]
        public string UsageEnumDescription
        {
            get
            {
                Type enumType = typeof(BrickUsageType);
                List<string> descriptions = [];

                foreach (BrickUsageType usage in Enum.GetValues<BrickUsageType>())
                {
                    // Skip the 'NoInformation' flag if it is not set
                    if (usage == BrickUsageType.NoInformation && !UsageEnum.HasFlag(BrickUsageType.NoInformation))
                    {
                        continue;
                    }

                    if (UsageEnum.HasFlag(usage) && usage != BrickUsageType.NoInformation)
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

                return descriptions.Count > 0 ? string.Join(", ", descriptions) : BrickUsageType.NoInformation.GetDescription();
            }
        }
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