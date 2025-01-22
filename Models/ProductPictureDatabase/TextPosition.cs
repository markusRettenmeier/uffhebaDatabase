using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models.ProductPictureDatabase
{
    public class TextPosition
    {
        public required string Text { get; set; }
        public int Height { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
    }
}
