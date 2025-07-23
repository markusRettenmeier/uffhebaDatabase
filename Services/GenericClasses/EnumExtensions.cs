using System.ComponentModel;
using System.Reflection;

namespace Sammlerplattform.Services.GenericClasses
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo? fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
