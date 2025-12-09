using ImageMagick;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;

namespace Sammlerplattform.Services.DatabaseProcesses.PictureProcesses
{
    public interface IProcessPicturePhysically
    {
        (int Statuscode, string Statusmessage) Save(CollectionItemPicture collectionItemPicture, int id, bool update);
        (int Statuscode, string Statusmessage) Delete(int id);
    }

    public partial class PhysicalPictureProcessor(IWebHostEnvironment hostEnvironment) : IProcessPicturePhysically
    {
        public (int Statuscode, string Statusmessage) Save(CollectionItemPicture collectionItemPicture, int id, bool update)
        {
            try
            {
                if (update)
                {
                    Delete(id);
                }
                string pathFile = SaveFileForOCR(collectionItemPicture.IFormFile, hostEnvironment);
                var fileStream = new FileStream(pathFile, FileMode.Open);
                MagickImage image = new(fileStream)
                {
                    Quality = 30
                };
                ImageSetWatermark("SammlerDB", image);
                ImageResizeAndMoveToFolder(collectionItemPicture.Frontside, id, image);

                return (200, "Success_PhysicalPicture_Saved");
            }
            catch (Exception ex)
            {
                return (500, "Error_Error_Ocurred");

            }
        }

        public (int Statuscode, string Statusmessage) Delete(int id)
        {
            try
            {
                string imgName = id + ".webp";
                DeletePicturesFromFolders(Pathes(imgName));
                imgName = id + ".png";
                DeletePicturesFromFolders(Pathes(imgName));

                return (200, "Success_PhysicalPicture_Deleted");
            }
            catch (Exception ex)
            {
                return (500, "Error_Error_Ocurred");
            }
        }

        public static void DeletePicturesFromFolders((string pathNormal, string pathSmall, string pathThumbnail) pathes)
        {
            File.Delete(pathes.pathNormal);
            File.Delete(pathes.pathSmall);
            File.Delete(pathes.pathThumbnail);
        }

        private void ImageResizeAndMoveToFolder(bool isfrontside, int id, MagickImage image)
        {
            var pathesWebp = Pathes(id + ".webp");
            var pathesPng = Pathes(id + ".png");
            if (File.Exists(pathesWebp.pathNormal))
            {
                File.Delete(pathesWebp.pathNormal);
            }
            image.Write(pathesWebp.pathNormal, MagickFormat.WebP);
            if (File.Exists(pathesPng.pathNormal))
            {
                File.Delete(pathesPng.pathNormal);
            }
            image.Write(pathesPng.pathNormal, MagickFormat.Png);

            if (isfrontside)
            {
                // Für kleine Version
                if (image.Width > image.Height)
                {
                    image.Scale(498, 322);
                }
                else if (image.Height > image.Width)
                {
                    image.Scale(498, 708);
                    image.Crop(498, 322);
                }
                if (File.Exists(pathesWebp.pathSmall))
                {
                    File.Delete(pathesWebp.pathSmall);
                }
                image.Write(pathesWebp.pathSmall, MagickFormat.WebP);
                if (File.Exists(pathesPng.pathSmall))
                {
                    File.Delete(pathesPng.pathSmall);
                }
                image.Write(pathesPng.pathSmall, MagickFormat.Png);

                //Thumbnail erstellen, wegen Normal immer Querformat
                if (image.Width > image.Height)
                {
                    image.Thumbnail(240, 153);
                }
                if (File.Exists(pathesWebp.pathThumbnail))
                {
                    File.Delete(pathesWebp.pathThumbnail);
                }
                image.Write(pathesWebp.pathThumbnail, MagickFormat.WebP);
                if (File.Exists(pathesPng.pathThumbnail))
                {
                    File.Delete(pathesPng.pathThumbnail);
                }
                image.Write(pathesPng.pathThumbnail, MagickFormat.Png);
            }
        }
        private static void ImageSetWatermark(string userName, MagickImage image)
        {
            MagickReadSettings readSettings = new()
            {
                Font = "Calibri",
                TextGravity = Gravity.Center,
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.LightGray,
                Height = 200,
                Width = 400
            };
            MagickImage watermark = new($"caption:{userName}", readSettings);
            watermark.Rotate(315.00);

            // Normal Version                        
            if (image.Width > image.Height)
            {
                image.Composite(watermark, Gravity.Center, 600, 350, CompositeOperator.Over);
                image.Composite(watermark, Gravity.Center, -600, -350, CompositeOperator.Over);
            }
            else if (image.Height > image.Width)
            {
                image.Composite(watermark, Gravity.Center, 420, 680, CompositeOperator.Over);
                image.Composite(watermark, Gravity.Center, -420, -680, CompositeOperator.Over);
            }
            else
            {
                image.Composite(watermark, Gravity.Center, 0, 0, CompositeOperator.Over);
            }
        }
        private (string pathNormal, string pathSmall, string pathThumbnail) Pathes(string imgName)
        {
            string wwwRootPath = hostEnvironment.WebRootPath;
            string pathNormal = Path.Combine(Path.Combine(wwwRootPath, Path.Combine("images", "Normal")), imgName);
            string pathSmall = Path.Combine(Path.Combine(wwwRootPath, Path.Combine("images", "Klein")), imgName);
            string pathThumbnail = Path.Combine(Path.Combine(wwwRootPath, Path.Combine("images", "Thumbnail")), imgName);

            return (pathNormal, pathSmall, pathThumbnail);
        }
        public static string SaveFileForOCR(IFormFile? fileToAnalyze, IWebHostEnvironment hostEnvironment)
        {
            if (fileToAnalyze == null)
            {
                return "";
            }

            MemoryStream ms;
            MagickImage image;

            string pathOriginal = Path.Combine(hostEnvironment.WebRootPath, Path.Combine("images", "Original"));
            string pathFile = Path.Combine(pathOriginal, RandomString(10) + DateTime.Now.ToString("_ddMMyyhhmmss") + ".tiff");

            ms = new MemoryStream();
            fileToAnalyze.CopyTo(ms);
            ms.Position = 0;
            image = new MagickImage(ms)
            {
                Format = MagickFormat.Tiff
            };
            image.Scale(640, 480);
            image.Write(pathFile);
            File.SetLastAccessTime(pathFile, DateTime.Now);

            return pathFile;
        }
        public static string RandomString(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
        }
    }
}
