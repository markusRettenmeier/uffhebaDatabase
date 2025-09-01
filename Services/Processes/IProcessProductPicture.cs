using ImageMagick;
using Sammlerplattform.Data;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Services.Picture;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessProductPicture
    {
        (ProductPicture productPicture, int statuscode, string message) Create(ProductPicture productPicture, BrickEntity brickEntity);
        (ProductPicture productPicture, int statuscode, string message) Edit(ProductPicture productPicture, BrickEntity brickEntity);
        (ProductPicture productPicture, int statuscode, string message) Delete(ProductPicture productPicture);
    }

    public class ProductPictureProcessor(IWebHostEnvironment hostEnvironment, IUnitOfWork unitOfWork, ILogger<ProductPictureProcessor> logger) : IProcessProductPicture
    {
        public (ProductPicture productPicture, int statuscode, string message) Create(ProductPicture productPicture, BrickEntity brickEntity)
        {
            if (brickEntity.UsingIdentityUser == null || string.IsNullOrEmpty(brickEntity.UsingIdentityUser.UserName))
            {
                return (productPicture, 302, "User fehlt.");
            }

            if (productPicture.Datei != null)
            {
                try
                {
                    productPicture.BrickEntityID = brickEntity.BrickEntityID;
                    ProductPicture newProductPicture = unitOfWork.ProductPictureRepository.Insert(productPicture);
                    unitOfWork.Save();

                    string imgName = newProductPicture.ProductPictureID.ToString() + "." + newProductPicture.FileExtension;
                    string fileName = PicturePreprocess.SaveFileForAnalysis(imgName, productPicture.Datei, hostEnvironment);
                    using MagickImage image = new(fileName);
                    image.Quality = 30;
                    image.Format = MagickFormat.Png;
                    ImageSetWatermark(brickEntity.UsingIdentityUser.UserName, image);
                    ImageResizeAndMoveToFolder(productPicture.Frontside, Pathes(imgName), image);

                    return (newProductPicture, 201, "Bild gespeichert.");
                }
                catch (Exception ex)
                {
                    //logger.LogError("ProcessAnalysisResultParameters publisher abgebrochen mit Exception {ex.Message}, fileName {fileName}.",
                    //            ex.Message, fileName);
                    return (productPicture, 500, "Es ist ein Fehler aufgetreten: " + ex.Message);
                }
            }

            return (productPicture, 302, "Bild leer.");
        }
        public (ProductPicture productPicture, int statuscode, string message) Edit(ProductPicture productPicture, BrickEntity brickEntity)
        {
            if (brickEntity.UsingIdentityUser == null || string.IsNullOrEmpty(brickEntity.UsingIdentityUser.UserName))
            {
                return (productPicture, 302, "User fehlt.");
            }

            ProductPicture? existingProductPicture = unitOfWork.ProductPictureRepository.GetByID(productPicture.ProductPictureID);
            if (existingProductPicture == null)
            {
                return (productPicture, 302, "Eintrag des Bildes in Datanbank nicht gefunden.");
            }
            if (existingProductPicture.PerspectiveInt != productPicture.PerspectiveInt)
            {
                try
                {
                    existingProductPicture.PerspectiveInt = productPicture.PerspectiveInt;
                    unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    //logger.LogError("ProcessAnalysisResultParameters publisher abgebrochen mit Exception {ex.Message}, fileName {fileName}.",
                    //            ex.Message, fileName);
                    return (productPicture, 500, "Es ist ein Fehler aufgetreten: " + ex.Message);
                }
            }

            if (productPicture.Datei != null)
            {
                string imgName = existingProductPicture.ProductPictureID.ToString() + "." + existingProductPicture.FileExtension;
                DeletePicturesFromFolders(Pathes(imgName));

                string fileName = PicturePreprocess.SaveFileForAnalysis(imgName, productPicture.Datei, hostEnvironment);
                using MagickImage image = new(fileName);
                image.Quality = 30;
                image.Format = MagickFormat.Png;
                ImageSetWatermark(brickEntity.UsingIdentityUser.UserName, image);
                ImageResizeAndMoveToFolder(productPicture.Frontside, Pathes(imgName), image);

                return (productPicture, 201, "Bild gespeichert.");
            }

            return (productPicture, 201, "Änderung erfolgreich.");
        }
        public (ProductPicture productPicture, int statuscode, string message) Delete(ProductPicture productPicture)
        {
            ProductPictureSearchParameterModel searchParameterModel = ParametersOperationToSearch(productPicture);
            ProductPicture? existingProductPicture = GetWithPredicate(searchParameterModel);

            if (existingProductPicture != null)
            {
                try
                {
                    unitOfWork.ProductPictureRepository.Delete(productPicture);
                    unitOfWork.Save();

                    string imgName = existingProductPicture.ProductPictureID.ToString() + "." + existingProductPicture.FileExtension;
                    DeletePicturesFromFolders(Pathes(imgName));
                }
                catch (Exception ex)
                {
                    logger.LogError("ProcessAnalysisResultParameters publisher abgebrochen mit Exception {ex.Message}, fileName {productPicture.ProductPictureID}.",
                                ex.Message, productPicture.ProductPictureID);
                    return (productPicture, 500, "Es ist ein Fehler aufgetreten: " + ex.Message);
                }
            }

            return (productPicture, 201, "Bild gelöscht.");
        }

        private static void DeletePicturesFromFolders((string pathNormal, string pathSmall, string pathThumbnail) pathes)
        {
            File.Delete(pathes.pathNormal);
            File.Delete(pathes.pathSmall);
            File.Delete(pathes.pathThumbnail);
        }

        private static void ImageResizeAndMoveToFolder(bool isfrontside, (string pathNormal, string pathSmall, string pathThumbnail) pathes, MagickImage image)
        {
            if (File.Exists(pathes.pathNormal))
            {
                File.Delete(pathes.pathNormal);
            }
            image.Write(pathes.pathNormal);

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
                if (File.Exists(pathes.pathSmall))
                {
                    File.Delete(pathes.pathSmall);
                }
                image.Write(pathes.pathSmall);

                //Thumbnail erstellen, wegen Normal immer Querformat
                if (image.Width > image.Height)
                {
                    image.Thumbnail(240, 153);
                }
                if (File.Exists(pathes.pathThumbnail))
                {
                    File.Delete(pathes.pathThumbnail);
                }
                image.Write(pathes.pathThumbnail);
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

        private static ProductPictureSearchParameterModel ParametersOperationToSearch(ProductPicture productPicture)
        {
            ProductPictureSearchParameterModel searchParameterModel = new();
            searchParameterModel.ProductPictureID.Add(productPicture.ProductPictureID);
            searchParameterModel.FileExtension.Add(productPicture.FileExtension);
            if (productPicture.BrickEntityID != null)
            {
                searchParameterModel.BrickEntityID.Add((int)productPicture.BrickEntityID);
            }

            searchParameterModel.Frontside = productPicture.Frontside;
            return searchParameterModel;
        }

        private ProductPicture? GetWithPredicate(ProductPictureSearchParameterModel searchParameterModel)
        {
            return unitOfWork.ProductPictureRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<ProductPicture>(searchParameterModel)).FirstOrDefault();
        }

        private (string pathNormal, string pathSmall, string pathThumbnail) Pathes(string imgName)
        {
            string wwwRootPath = hostEnvironment.WebRootPath;
            string pathNormal = Path.Combine(Path.Combine(wwwRootPath, Path.Combine("images", "Normal")), imgName);
            string pathSmall = Path.Combine(Path.Combine(wwwRootPath, Path.Combine("images", "Klein")), imgName);
            string pathThumbnail = Path.Combine(Path.Combine(wwwRootPath, Path.Combine("images", "Thumbnail")), imgName);

            return (pathNormal, pathSmall, pathThumbnail);
        }
    }
}
