using System.Drawing;

namespace Sammlerplattform.Services
{
    public class ColorConverter
    {
        public static string? ArgbToHex(int? argb)
        {
            if (argb != null)
            {
                string hex = Color.FromArgb((int)argb).Name;
                int hexLength = hex.Length;
                for (int i = 0; i < 6 - hexLength; i++)
                {
                    hex = "0" + hex;
                }
                return "#" + hex;
            }
            else
            {
                return null;
            }
        }
    }
}
