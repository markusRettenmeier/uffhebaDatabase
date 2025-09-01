using ImageMagick;
namespace Sammlerplattform.Services.Picture
{
    public partial class PicturePreprocess
    {
        public static string SaveFileForAnalysis(IFormFile fileToAnalyze, IWebHostEnvironment hostEnvironment)
        {
            Random random;
            string pathTemp, fileName, pathFile;
            MemoryStream ms;
            MagickImage image;
            random = new Random();
            pathTemp = Path.Combine(hostEnvironment.WebRootPath, "images/Zwischenablage");
            fileName = random.Next(1, 1000) + DateTime.Now.ToString("_ddMMyyhhmmss");
            pathFile = Path.Combine(pathTemp, fileName + ".png");

            ms = new MemoryStream();
            fileToAnalyze.CopyTo(ms);
            ms.Position = 0;
            image = new MagickImage(ms)
            {
                Format = MagickFormat.Png
            };
            image.Write(pathFile);
            File.SetLastAccessTime(pathFile, DateTime.Now);

            return pathFile;
        }

        public static string SaveFileForAnalysis(string? fileName, IFormFile fileToAnalyze, IWebHostEnvironment hostEnvironment)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName), "File name cannot be null.");
            }

            MemoryStream ms;
            MagickImage image;

            string pathOriginal = Path.Combine(hostEnvironment.WebRootPath, Path.Combine("images", "Original"));
            string pathFile = Path.Combine(pathOriginal, fileName);

            ms = new MemoryStream();
            fileToAnalyze.CopyTo(ms);
            ms.Position = 0;
            image = new MagickImage(ms)
            {
                Format = MagickFormat.Png
            };
            image.Write(pathFile);
            File.SetLastAccessTime(pathFile, DateTime.Now);

            return pathFile;
        }
    }
}
