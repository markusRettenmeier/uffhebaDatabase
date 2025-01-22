using ImageMagick;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Controllers.PictureAnaylsis;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using System.Globalization;
using System.IO.Compression;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    [Authorize(Policy = "SubscribedDiskspacePolicy")]
    public class PostcardDatabaseController(IWebHostEnvironment hostEnvironment, UserManager<UsingIdentityUser> userManager,
        DbIdentityContext dbIdentityContext, ILogger<PostcardDatabaseController> logger, IProcessCity processCity) : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;
        private readonly DbIdentityContext _dbIdentityContext = dbIdentityContext;
        private readonly ILogger<PostcardDatabaseController> _logger = logger;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("de-DE");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public async Task<ActionResult> AdministerCollectionPostcard(string statusMessage, PostcardSearchParameters postcardSearchParameters)
        {
            ExpressionStarter<PostcardModel> predicate = PredicateBuilder.New<PostcardModel>();
            string? userId = _userManager.GetUserId(User);
            IQueryable<PostcardModel> PostcardIQueryable = from user in _userManager.Users
                                                           join pe in _dbIdentityContext.PostcardEntity
                                                           on user.Id equals pe.UsingIdentityUsers_ID
                                                           join Scan in _dbIdentityContext.ProductPicture
                                                           on pe.PostcardEntity_ID equals Scan.PostcardEntity_ID
                                                           join pp in _dbIdentityContext.PostcardPotential
                                                                .Include(c => c.CityList)
                                                                .ThenInclude(o => o.PostalcodeICollection)
                                                                .Include(pp => pp.CityList)
                                                                .ThenInclude(city => city.CityNOeconymICollection.Where(x => x.CurrentName)).ThenInclude(x => x.Oeconym)
                                                           on pe.PostcardPotential_ID equals pp.PostcardPotential_ID
                                                           where user.Id == userId
                                                           && Scan.Frontside == true
                                                           select new PostcardModel
                                                           {
                                                               PostcardEntity = pe
                                                           ,
                                                               PostcardPotential = pp
                                                           ,
                                                               ProductScan = Scan
                                                           ,
                                                               UsingIdentityUser = user
                                                           };

            if (postcardSearchParameters.SearchCity != null)
            {
                foreach (string cityName in postcardSearchParameters.SearchCity)
                {
                    if (!string.IsNullOrEmpty(cityName))
                    {
                        predicate = predicate.Or(x => x.PostcardPotential.CityList.Any(x => x.CityNOeconymICollection.Any(y => y.Oeconym.OeconymName == cityName)));
                    }
                }
            }

            if (postcardSearchParameters.SearchPostalcode != null)
            {
                foreach (string postalcode in postcardSearchParameters.SearchPostalcode)
                {
                    if (!string.IsNullOrEmpty(postalcode))
                    {
                        Postalcode? selectPostalcode = (from p in _dbIdentityContext.Postalcode.Include(c => c.CityICollection)
                                                        where p.PostalcodeNumber == postalcode
                                                        select p).FirstOrDefault();
                        if (selectPostalcode != null && selectPostalcode.CityICollection != null)
                        {
                            foreach (City city in selectPostalcode.CityICollection)
                            {
                                predicate = predicate.Or(x => x.PostcardPotential.CityList.Contains(city));
                            }
                        }
                        else
                        {
                            predicate = predicate.Or(x => x.PostcardPotential.CityList.Contains(new()));
                        }
                    }
                }
            }
            if (predicate.IsStarted == true)
            {
                PostcardIQueryable = PostcardIQueryable.Where(predicate);
            }

            List<PostcardModel> postcardList = await PostcardIQueryable.ToListAsync();

            _ = Directory.CreateDirectory(Path.Combine(_hostEnvironment.WebRootPath, "images/Klein"));
            _ = Directory.CreateDirectory(Path.Combine(_hostEnvironment.WebRootPath, "images/Normal"));
            _ = Directory.CreateDirectory(Path.Combine(_hostEnvironment.WebRootPath, "images/Thumbnail"));

            ViewData["StatusMessage"] = statusMessage;
            ViewData["userId"] = userId;

            return View(postcardList);
        }

        public IActionResult CreatePostcard()
        {
            ViewData["BackToList"] = "CreatePostcard";

            return View();
        }
        public async Task<ActionResult> CreatePostcardSubmit(PostcardAnalyzeResultParameters parameters, PostcardModel postcardModel, IFormFile Frontside, IFormFile Backside, string pathFrontside, string pathBackside)
        {
            UsingIdentityUser user = await _userManager.GetUserAsync(User) ?? throw new NullReferenceException("user");
            int id = 0;
            string statusMessage = string.Empty;

            bool isComingFromAnalysis = false;
            if (!string.IsNullOrEmpty(pathFrontside))
            {
                isComingFromAnalysis = true;
                DbActionsPostcard dbChangesPostcard = new(_dbIdentityContext, _userManager, processCity, _logger);
                dbChangesPostcard.ProcessAnalysisResultParameters(parameters, postcardModel);
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                PostcardImprint? newPostcardImprint = null;
                if (postcardModel.HasImage)
                {
                    newPostcardImprint = new();
                    if (!string.IsNullOrEmpty(postcardModel.ColorImage))
                    {
                        newPostcardImprint.ColorImage_ID = int.Parse(postcardModel.ColorImage[1..], System.Globalization.NumberStyles.HexNumber);
                    }

                    newPostcardImprint.Era_ID = postcardModel.PostcardImprint.Era_ID;
                    newPostcardImprint.ImagePerception = postcardModel.PostcardImprint.ImagePerception;
                    newPostcardImprint.ArtistAuthor_ID = postcardModel.PostcardImprint.ArtistAuthor_ID;
                    newPostcardImprint.ImageYear = postcardModel.PostcardImprint.ImageYear;
                    newPostcardImprint.ColorProcessing = postcardModel.PostcardImprint.ColorProcessing;
                    newPostcardImprint.Buildings = postcardModel.PostcardImprint.Buildings;
                    newPostcardImprint.FullScreen = postcardModel.PostcardImprint.FullScreen;
                    newPostcardImprint.PictureCount = postcardModel.PostcardImprint.PictureCount;
                    newPostcardImprint.Passepartout = postcardModel.PostcardImprint.Passepartout;
                    switch (postcardModel.PostcardPotential.Formats)
                    {
                        // Format klein T74
                        case 1:
                            newPostcardImprint.Width = 10.5;
                            newPostcardImprint.Height = 14.8;
                            break;
                        // Format groß T76
                        case 2:
                            newPostcardImprint.Width = 10.5;
                            newPostcardImprint.Height = 21;
                            break;
                        // Format Übergroß
                        case 3:
                            newPostcardImprint.Width = 12.5;
                            newPostcardImprint.Height = 23.5;
                            break;
                    }

                    //there is anyway a limited number of choice, so it would rarely create a new databse entry
                    Printing? newPrinting = null;
                    if (postcardModel.Printing.Technique > 0 || postcardModel.Printing.Style > 0)
                    {
                        Printing? printingSelect = await (from p in _dbIdentityContext.Printing
                                                          where p.Technique == postcardModel.Printing.Technique
                                                          && p.Style == postcardModel.Printing.Style
                                                          select p).FirstOrDefaultAsync();

                        if (printingSelect != null)
                        {
                            newPrinting = printingSelect;
                        }
                        else
                        {
                            newPrinting = new()
                            {
                                Technique = postcardModel.Printing.Technique,
                                Style = postcardModel.Printing.Style
                            };
                            _ = _dbIdentityContext.Add(newPrinting);
                            _ = await _dbIdentityContext.SaveChangesAsync();
                        }
                        if (newPrinting != null)
                        {
                            newPostcardImprint.Printing_ID = newPrinting.Printing_ID;
                        }
                    }
                    _ = _dbIdentityContext.Add(newPostcardImprint);
                    _ = await _dbIdentityContext.SaveChangesAsync();
                }

                PostcardPotential newPostcardPotential = new()
                {
                    Formats = postcardModel.PostcardPotential.Formats
                    //,
                    //ProductionYear = postcardModel.PostcardPotential.ProductionYear
                    ,
                    CardType = postcardModel.PostcardPotential.CardType
                    ,
                    SerialNumber = postcardModel.PostcardPotential.SerialNumber
                    ,
                    CardSeries = postcardModel.PostcardPotential.CardSeries
                };
                foreach (int cityID in postcardModel.CityIDList)
                {
                    City? city = (from c in _dbIdentityContext.City where c.City_ID == cityID select c).FirstOrDefault();
                    if (city != null)
                    {
                        newPostcardPotential.CityList.Add(city);
                        city.PostcardPotentialList.Add(newPostcardPotential);
                    }
                }
                //If postcardModel gets filled by parameters
                foreach ((City City, List<Postalcode> PostalcodeList, Geography Geography) cityTuple in postcardModel.CityTupleList)
                {
                    newPostcardPotential.CityList.Add(cityTuple.City);
                    City? city = (from c in _dbIdentityContext.City where c.City_ID == cityTuple.City.City_ID select c).FirstOrDefault();
                    city?.PostcardPotentialList.Add(newPostcardPotential);
                }
                if (newPostcardImprint != null && newPostcardImprint.Image_ID > 0)
                {
                    newPostcardPotential.PostcardImprint_ID = newPostcardImprint.Image_ID;
                }

                _ = _dbIdentityContext.Add(newPostcardPotential);
                _ = await _dbIdentityContext.SaveChangesAsync();

                Person? newSender = new() { Name = string.Empty };
                if (postcardModel.HasSender)
                {
                    Person? checkExistingSender = (from p in _dbIdentityContext.Person
                                                   where p.Name == postcardModel.PersonSender.Name
                                                   select p).FirstOrDefault();
                    if (checkExistingSender == null &&
                        postcardModel.PersonSender.Name != null)
                    {
                        newSender.Name = postcardModel.PersonSender.Name;
                        _ = _dbIdentityContext.Add(newSender);
                        _ = await _dbIdentityContext.SaveChangesAsync();
                    }
                    else
                    {
                        newSender = checkExistingSender;
                    }
                }

                Person? newReceiver = null;
                if (postcardModel.HasReceiver)
                {
                    Person? selectPerson = await (from pe in _dbIdentityContext.Person.Include(x => x.City)
                                                  where pe.Name == postcardModel.PersonReceiver.Name
                                                  && pe.City_ID == postcardModel.PersonReceiver.City_ID
                                                  select pe).FirstOrDefaultAsync();
                    if (selectPerson == null)
                    {
                        newReceiver = new()
                        {
                            Name = postcardModel.PersonReceiver.Name,
                            City_ID = postcardModel.PersonReceiver.City_ID
                        };
                        _ = _dbIdentityContext.Add(newReceiver);
                        _ = await _dbIdentityContext.SaveChangesAsync();
                    }
                    else
                    {
                        newReceiver = selectPerson;
                    }
                }

                //ManufacturingDate newManufacturingDate = new()
                //{
                //    ExactYear = postcardModel.
                //}

                PostcardEntity newPostcardEntity = new()
                {
                    PostcardPotential_ID = newPostcardPotential.PostcardPotential_ID,
                    FilingLocation = postcardModel.PostcardEntity.FilingLocation,
                    Charge = postcardModel.PostcardEntity.Charge,
                    ConditionInt = (int)postcardModel.PostcardEntity.ConditionEnum,
                    DateInText = postcardModel.PostcardEntity.DateInText,
                    UsingIdentityUsers_ID = user.Id,
                    Text = postcardModel.PostcardEntity.Text,
                    MaterialInt = (int)postcardModel.PostcardEntity.MaterialEnum
                };
                decimal? priceDecimal = null;
                if (!string.IsNullOrEmpty(postcardModel.PriceString))
                {
                    priceDecimal = decimal.Parse(postcardModel.PriceString);
                }

                newPostcardEntity.Price = priceDecimal;
                if (!string.IsNullOrEmpty(postcardModel.ColorRALPrinting))
                {
                    newPostcardEntity.ColorRALPrintingBackside = int.Parse(postcardModel.ColorRALPrinting[1..], System.Globalization.NumberStyles.HexNumber);
                }

                if (!string.IsNullOrEmpty(postcardModel.ColorRALWriting))
                {
                    newPostcardEntity.ColorRALWritingFrontside = int.Parse(postcardModel.ColorRALWriting[1..], System.Globalization.NumberStyles.HexNumber);
                }

                if (newSender != null && newSender.Person_ID != 0)
                {
                    newPostcardEntity.Sender_ID = newSender.Person_ID;
                }

                if (newReceiver != null && newReceiver.Person_ID != 0)
                {
                    newPostcardEntity.Receiver_ID = newReceiver.Person_ID;
                }

                _ = _dbIdentityContext.Add(newPostcardEntity);
                _ = await _dbIdentityContext.SaveChangesAsync();

                foreach (string? publisher in postcardModel.ManufactoryIDCityIDList)
                {
                    if (publisher is not null && publisher.Contains("§§"))
                    {
                        string[] splittedPublisher = publisher.Split("§§");
                        if (string.IsNullOrEmpty(splittedPublisher[0]))
                        {
                            continue;
                        }

                        int? cityId = null;
                        IQueryable<PostcardEntityNManufactoryNCity> selectPEPC = from pepc in _dbIdentityContext.PostcardEntityNManufactoryNCity
                                                                                 where pepc.PostcardEntity_ID == newPostcardEntity.PostcardEntity_ID
                                                                                 && pepc.Publisher_ID == short.Parse(splittedPublisher[0])
                                                                                 select pepc;
                        if (!string.IsNullOrEmpty(splittedPublisher[1]))
                        {
                            cityId = short.Parse(splittedPublisher[1]);
                            selectPEPC = selectPEPC.Where(p => p.City_ID == cityId);
                        }

                        if (selectPEPC.FirstOrDefault() == null)
                        {
                            PostcardEntityNManufactoryNCity newPostcardEntityPublishrCity = new()
                            {
                                PostcardEntity_ID = newPostcardEntity.PostcardEntity_ID,
                                Publisher_ID = short.Parse(splittedPublisher[0]),
                                City_ID = cityId
                            };
                            _ = _dbIdentityContext.Add(newPostcardEntityPublishrCity);
                        }
                    }
                }
                /// If coming from Analysis
                foreach ((Manufactory manufactory, City? city, List<City> cityList) in postcardModel.ManufactoryTupleList)
                {
                    if (manufactory != null)
                    {
                        IQueryable<PostcardEntityNManufactoryNCity> selectPEPC = from pepc in _dbIdentityContext.PostcardEntityNManufactoryNCity
                                                                                 where pepc.PostcardEntity_ID == newPostcardEntity.PostcardEntity_ID
                                                                                 && pepc.Publisher_ID == manufactory.Manufactory_ID
                                                                                 select pepc;
                        if (city != null)
                        {
                            selectPEPC = selectPEPC.Where(p => p.City_ID == city.City_ID);
                        }

                        if (selectPEPC.FirstOrDefault() == null)
                        {
                            PostcardEntityNManufactoryNCity newPostcardEntityPublishrCity = new()
                            {
                                PostcardEntity_ID = newPostcardEntity.PostcardEntity_ID,
                                Publisher_ID = manufactory.Manufactory_ID
                            };
                            if (city != null)
                            {
                                newPostcardEntityPublishrCity.City_ID = city.City_ID;
                            }

                            _ = _dbIdentityContext.Add(newPostcardEntityPublishrCity);
                        }
                    }
                }
                _ = await _dbIdentityContext.SaveChangesAsync();

                if (user.UserName == null)
                {
                    throw new NullReferenceException("userName zu Id " + user.Id);
                }

                string currentFileName = string.IsNullOrEmpty(pathFrontside) ? PicturePreprocess.SaveFileForAnalysis(Frontside, _hostEnvironment) : pathFrontside;
                _ = await CreateProductPicture(currentFileName, true, newPostcardEntity.PostcardEntity_ID, user, false);

                if (Backside != null || !string.IsNullOrEmpty(pathBackside))
                {
                    currentFileName = string.IsNullOrEmpty(pathBackside) && Backside != null
                        ? PicturePreprocess.SaveFileForAnalysis(Backside, _hostEnvironment)
                        : pathBackside;
                    _ = await CreateProductPicture(currentFileName, false, newPostcardEntity.PostcardEntity_ID, user, false);
                }
                id = newPostcardPotential.PostcardPotential_ID;

                statusMessage = "Erstellung erfolgreich.";
                scope.Complete();
            }
            catch (TransactionAbortedException ex)
            {
                //DeletePictures
                _logger.LogError("CreatePostcardSubmit abgebrochen mit Exception {ex.Message}", ex);
                statusMessage = "Erstellung wurde abgebrochen. Fehler: " + ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreatePostcardSubmit abgebrochen mit Exception {ex.Message}", ex);
                statusMessage = "Erstellung wurde abgebrochen. Fehler: " + ex.Message;
            }

            return isComingFromAnalysis
                ? RedirectToAction(nameof(EditPostcard), new { id })
                : (ActionResult)RedirectToAction("AdministerCollectionPostcard", "PostcardDatabase", new { statusMessage });
        } // END CreatePostcard


        private async Task<ProductPicture> CreateProductPicture(string fileName, bool Frontside, int? PostcardEntity_ID, UsingIdentityUser user, bool replaceOldScan)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string pathNormal = Path.Combine(wwwRootPath, Path.Combine("images","Normal"));
            string pathSmall = Path.Combine(wwwRootPath, Path.Combine("images","Klein"));
            string pathThumbnail = Path.Combine(wwwRootPath, Path.Combine("images","Thumbnail"));
            string pathFile = string.Empty;
            string pathOriginal = Path.Combine(wwwRootPath, Path.Combine("images","Original"));
            DateTime currentDate = DateTime.Now;
            ProductPicture productScan = new();

            if (!string.IsNullOrEmpty(fileName))
            {
                string fileExtension = Path.GetExtension(fileName);
                if (fileExtension == string.Empty)
                {
                    fileExtension = "png";
                }

                string ImgName = string.Empty;

                if (replaceOldScan)
                {
                    ProductPicture selectScan = (from s in _dbIdentityContext.ProductPicture
                                               where s.PostcardEntity_ID == PostcardEntity_ID
                                               && s.Frontside == Frontside
                                               select s).FirstOrDefault() ?? throw new NullReferenceException("scanSelect");
                    ImgName = selectScan.ProductPicture_ID.ToString() + "." + selectScan.FileExtension;
                    productScan = selectScan;
                }
                else
                {
                    ProductPicture newProductScan = new()
                    {
                        FileExtension = fileExtension,
                        Frontside = Frontside
                    };

                    if (PostcardEntity_ID != null)
                    {
                        newProductScan.PostcardEntity_ID = (int)PostcardEntity_ID;
                    }

                    _ = _dbIdentityContext.Add(newProductScan);
                    try
                    {
                        _ = await _dbIdentityContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("ProcessAnalysisResultParameters publisher abgebrochen mit Exception {ex}, name {scanName[1].Trim()}.",
                                    ex.Message, fileExtension);
                        if(ex.InnerException != null)
                            throw new DbUpdateException(ex.InnerException.Message + " filename: " + fileName + " fileExtension: " + fileExtension + " Frontside: " + Frontside);
                        else
                            throw new DbUpdateException(ex.Message);
                    }

                    ImgName = newProductScan.ProductPicture_ID.ToString() + "." + newProductScan.FileExtension;
                    productScan = newProductScan;
                }

                MagickReadSettings readSettings = new()
                {
                    Font = "Calibri",
                    TextGravity = Gravity.Center,
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.LightGray,
                    Height = 200, // height of text box
                    Width = 400 // width of text box                        
                };
                MagickImage watermark = new($"caption:{user.UserName}", readSettings);
                watermark.Rotate(315.00);

                using MagickImage image = new(fileName);
                image.Quality = 30;
                image.Format = MagickFormat.Png;

                // Normal Version                        
                if (image.Width > image.Height)
                {
                    //Add the watermark layer on top of the background image
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

                pathFile = Path.Combine(pathNormal, ImgName);
                if (replaceOldScan)
                {
                    System.IO.File.Delete(pathFile + ".png");
                }
                image.Write(pathFile);

                if (Frontside)
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
                    pathFile = Path.Combine(pathSmall, ImgName);
                    if (replaceOldScan)
                    {
                        System.IO.File.Delete(pathFile + ".png");
                    }

                    image.Write(pathFile);

                    //Thumbnail erstellen, wegen Normal immer Querformat
                    if (image.Width > image.Height)
                    {
                        image.Thumbnail(240, 153);
                    }
                    pathFile = Path.Combine(pathThumbnail, ImgName);
                    if (replaceOldScan)
                    {
                        System.IO.File.Delete(pathFile + ".png");
                    }

                    image.Write(pathFile);
                }

                System.IO.File.Move(fileName, Path.Combine(pathOriginal, ImgName));
            }

            return productScan;
        }

        public ActionResult EditPostcard(int id, string statusMessage)
        {
            string? userId = _userManager.GetUserId(User);
            UsingIdentityUser? userAllowed = (from user in _userManager.Users
                                              join postcards in _dbIdentityContext.PostcardEntity
                                              on user.Id equals postcards.UsingIdentityUsers_ID
                                              join pso in _dbIdentityContext.PostcardPotential
                                              on postcards.PostcardPotential_ID equals pso.PostcardPotential_ID
                                              where user.Id == userId
                                              && pso.PostcardPotential_ID == id
                                              select user).FirstOrDefault();
            if (userAllowed != null && userId != null)
            {
                DbActionsPostcard dbChangesPostcard = new(_dbIdentityContext, _userManager, processCity, _logger);
                PostcardModel selectPostcard = dbChangesPostcard.QueryPostcardModel(userId).Where(x => x.PostcardPotential.PostcardPotential_ID.Equals(id)).First();

                if (selectPostcard.PostcardImprint != null && selectPostcard.PostcardImprint.Image_ID > 0)
                {
                    selectPostcard.HasImage = true;
                }

                if (selectPostcard.PersonSender != null && selectPostcard.PersonSender.Person_ID > 0)
                {
                    selectPostcard.HasSender = true;
                }

                if (selectPostcard.PersonReceiver != null && selectPostcard.PersonReceiver.Person_ID > 0)
                {
                    selectPostcard.HasReceiver = true;
                }

                ViewData["BackToList"] = "EditPostcard";
                ViewData["SourceId"] = selectPostcard.PostcardPotential.PostcardPotential_ID;
                ViewData["StatusMessage"] = statusMessage;

                return View(selectPostcard);
            }
            else
            {
                return RedirectToAction("AdministerCollectionPostcard", "PostcardDatabase");
            }
        }
        public async Task<IActionResult> EditPostcardSubmit(IFormFile Frontside, IFormFile Backside, PostcardModel postcardModel)
        {
            string statusMessage = string.Empty;

            UsingIdentityUser user = await _userManager.GetUserAsync(User) ?? throw new NullReferenceException("user in EditPostcardSubmit");
            if (user.UserName == null)
            {
                ArgumentNullException argumentNullExceptionEditPostcard = new(user.UserName, "UserName fehlt");
                throw argumentNullExceptionEditPostcard;
            }

            PostcardPotential selectPostcardPotential = (from p in _dbIdentityContext.PostcardPotential.Include(c => c.CityList)
                                                         where p.PostcardPotential_ID == postcardModel.PostcardPotential.PostcardPotential_ID
                                                         select p).First();

            try
            {
                using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

                PostcardImprint? newPostcardImprint = null;
                if (postcardModel.HasImage)
                {
                    PostcardImprint? selectImageImprint = (from p in _dbIdentityContext.PostcardImprint
                                                           where p.Image_ID == postcardModel.PostcardImprint.Image_ID
                                                           select p).FirstOrDefault();
                    if (selectImageImprint != null)
                    {
                        selectImageImprint.PictureCount = postcardModel.PostcardImprint.PictureCount;
                        selectImageImprint.FullScreen = postcardModel.PostcardImprint.FullScreen;
                        selectImageImprint.Passepartout = postcardModel.PostcardImprint.Passepartout;
                        selectImageImprint.CirculationSize = postcardModel.PostcardImprint.CirculationSize;
                        selectImageImprint.ImagePerception = postcardModel.PostcardImprint.ImagePerception;
                        selectImageImprint.ArtistAuthor_ID = postcardModel.PostcardImprint.ArtistAuthor_ID;
                        selectImageImprint.ImageYear = postcardModel.PostcardImprint.ImageYear;
                        selectImageImprint.Era_ID = postcardModel.PostcardImprint.Era_ID;
                        selectImageImprint.ColorProcessing = postcardModel.PostcardImprint.ColorProcessing;
                        selectImageImprint.Buildings = postcardModel.PostcardImprint.Buildings;
                        if (!string.IsNullOrEmpty(postcardModel.ColorImage))
                        {
                            selectImageImprint.ColorImage_ID = int.Parse(postcardModel.ColorImage[1..], System.Globalization.NumberStyles.HexNumber);
                        }

                        switch (postcardModel.PostcardPotential.Formats)
                        {
                            // Format klein T74
                            case 1:
                                selectImageImprint.Width = 10.5;
                                selectImageImprint.Height = 14.8;
                                break;
                            // Format groß T76
                            case 2:
                                selectImageImprint.Width = 10.5;
                                selectImageImprint.Height = 21;
                                break;
                            // Format Übergroß
                            case 3:
                                selectImageImprint.Width = 12.5;
                                selectImageImprint.Height = 23.5;
                                break;
                        }

                        if (selectImageImprint.Printing_ID != null)
                        {
                            Printing selectPrinting = (from p in _dbIdentityContext.Printing
                                                       where p.Printing_ID == selectImageImprint.Printing_ID
                                                       select p).First();
                            if ((postcardModel.Printing.Technique > 0 && selectPrinting.Technique != postcardModel.Printing.Technique)
                                || (postcardModel.Printing.Style > 0 && selectPrinting.Style != postcardModel.Printing.Style))
                            {
                                Printing? newPrinting = null;
                                Printing? printingSelect = (from p in _dbIdentityContext.Printing
                                                            where p.Technique == postcardModel.Printing.Technique
                                                            && p.Style == postcardModel.Printing.Style
                                                            select p).FirstOrDefault();

                                if (printingSelect != null)
                                {
                                    newPrinting = printingSelect;
                                }
                                else
                                {
                                    newPrinting = new()
                                    {
                                        Technique = postcardModel.Printing.Technique,
                                        Style = postcardModel.Printing.Style
                                    };
                                    _ = _dbIdentityContext.Add(newPrinting);
                                    _ = await _dbIdentityContext.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            //there is anyway a limited number of choice, so it would rarely create a new databse entry
                            Printing? newPrinting = null;
                            if (postcardModel.Printing.Technique > 0 || postcardModel.Printing.Style > 0)
                            {
                                Printing? printingSelect = (from p in _dbIdentityContext.Printing
                                                            where p.Technique == postcardModel.Printing.Technique
                                                            && p.Style == postcardModel.Printing.Style
                                                            select p).FirstOrDefault();

                                if (printingSelect != null)
                                {
                                    newPrinting = printingSelect;
                                }
                                else
                                {
                                    newPrinting = new()
                                    {
                                        Technique = postcardModel.Printing.Technique,
                                        Style = postcardModel.Printing.Style
                                    };
                                    _ = _dbIdentityContext.Add(newPrinting);
                                    _ = await _dbIdentityContext.SaveChangesAsync();
                                }
                            }
                            if (newPrinting != null)
                            {
                                selectImageImprint.Printing_ID = newPrinting.Printing_ID;
                            }
                        }
                    }
                    else
                    {
                        newPostcardImprint = new()
                        {
                            PictureCount = postcardModel.PostcardImprint.PictureCount,
                            FullScreen = postcardModel.PostcardImprint.FullScreen,
                            Passepartout = postcardModel.PostcardImprint.Passepartout,
                            CirculationSize = postcardModel.PostcardImprint.CirculationSize,
                            ImagePerception = postcardModel.PostcardImprint.ImagePerception,
                            ArtistAuthor_ID = postcardModel.PostcardImprint.ArtistAuthor_ID,
                            ImageYear = postcardModel.PostcardImprint.ImageYear,
                            Era_ID = postcardModel.PostcardImprint.Era_ID,
                            ColorProcessing = postcardModel.PostcardImprint.ColorProcessing
                        };
                        if (!string.IsNullOrEmpty(postcardModel.ColorImage))
                        {
                            newPostcardImprint.ColorImage_ID = int.Parse(postcardModel.ColorImage[1..], System.Globalization.NumberStyles.HexNumber);
                        }

                        switch (postcardModel.PostcardPotential.Formats)
                        {
                            // Format klein T74
                            case 1:
                                newPostcardImprint.Width = 10.5;
                                newPostcardImprint.Height = 14.8;
                                break;
                            // Format groß T76
                            case 2:
                                newPostcardImprint.Width = 10.5;
                                newPostcardImprint.Height = 21;
                                break;
                            // Format Übergroß
                            case 3:
                                newPostcardImprint.Width = 12.5;
                                newPostcardImprint.Height = 23.5;
                                break;
                        }

                        //there is anyway a limited number of choice, so it would rarely create a new databse entry
                        Printing? newPrinting = null;
                        if (postcardModel.Printing.Technique > 0 || postcardModel.Printing.Style > 0)
                        {
                            Printing? printingSelect = (from p in _dbIdentityContext.Printing
                                                        where p.Technique == postcardModel.Printing.Technique
                                                        && p.Style == postcardModel.Printing.Style
                                                        select p).FirstOrDefault();

                            if (printingSelect != null)
                            {
                                newPrinting = printingSelect;
                            }
                            else
                            {
                                newPrinting = new()
                                {
                                    Technique = postcardModel.Printing.Technique,
                                    Style = postcardModel.Printing.Style
                                };
                                _ = _dbIdentityContext.Add(newPrinting);
                                _ = await _dbIdentityContext.SaveChangesAsync();
                            }
                            if (newPrinting != null)
                            {
                                newPostcardImprint.Printing_ID = newPrinting.Printing_ID;
                            }
                        }
                        _ = _dbIdentityContext.Add(newPostcardImprint);
                    }
                }
                else if (postcardModel.PostcardImprint.Image_ID > 0)
                {
                    selectPostcardPotential.PostcardImprint_ID = null;
                    _ = await _dbIdentityContext.SaveChangesAsync();

                    List<PostcardPotential> selectAllPotentialsWithImprint = [.. (from p in _dbIdentityContext.PostcardPotential
                                                                              where p.PostcardImprint_ID == postcardModel.PostcardImprint.Image_ID
                                                                              select p)];
                    if (selectAllPotentialsWithImprint == null || selectAllPotentialsWithImprint.Count == 0)
                    {
                        PostcardImprint selectImageImprint = (from p in _dbIdentityContext.PostcardImprint
                                                              where p.Image_ID == postcardModel.PostcardImprint.Image_ID
                                                              select p).First();
                        _ = _dbIdentityContext.Remove(selectImageImprint);
                    }
                }
                else
                {
                    // Do Nothing
                }
                _ = await _dbIdentityContext.SaveChangesAsync();

                selectPostcardPotential.Formats = postcardModel.PostcardPotential.Formats;
                selectPostcardPotential.CardType = postcardModel.PostcardPotential.CardType;
                selectPostcardPotential.SerialNumber = postcardModel.PostcardPotential.SerialNumber;
                selectPostcardPotential.CardSeries = postcardModel.PostcardPotential.CardSeries;
                List<int> cityIdsCurrentList = [];
                foreach (City city in selectPostcardPotential.CityList)
                {
                    cityIdsCurrentList.Add(city.City_ID);
                }
                List<int> cityIDsInsertList = [];
                foreach (int cityID in postcardModel.CityIDList)
                {
                    City? city = (from c in _dbIdentityContext.City where c.City_ID.Equals(cityID) select c).FirstOrDefault();
                    if (city != null && !selectPostcardPotential.CityList.Contains(city))
                    {
                        selectPostcardPotential.CityList.Add(city);
                    }
                    cityIDsInsertList.Add(cityID);
                }
                foreach (int currentId in cityIdsCurrentList)
                {
                    if (!cityIDsInsertList.Contains(currentId))
                    {
                        City selectCity = (from c in _dbIdentityContext.City
                                           where c.City_ID == currentId
                                           select c).First();
                        _ = selectPostcardPotential.CityList.Remove(selectCity);
                    }
                }
                if (newPostcardImprint != null)
                {
                    selectPostcardPotential.PostcardImprint_ID = newPostcardImprint.Image_ID;
                }

                _ = await _dbIdentityContext.SaveChangesAsync();

                PostcardEntity selectPostcardEntity = (from p in _dbIdentityContext.PostcardEntity
                                                       where p.PostcardPotential_ID == postcardModel.PostcardPotential.PostcardPotential_ID
                                                       select p).First();

                Person? newSender = null;
                if (postcardModel.HasSender)
                {
                    if (postcardModel.PersonSender is not null)
                    {
                        if (postcardModel.PersonSender.Person_ID == 0)
                        {
                            if (postcardModel.PersonSender.Name != null)
                            {
                                newSender = new()
                                {
                                    Name = postcardModel.PersonSender.Name
                                };
                                _ = _dbIdentityContext.Add(newSender);
                                _ = await _dbIdentityContext.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            Person? SenderSQL = await (from p in _dbIdentityContext.Person
                                                       where p.Person_ID == postcardModel.PersonSender.Person_ID
                                                       select p).FirstOrDefaultAsync();
                            newSender = SenderSQL;
                        }
                    }
                }
                else if (postcardModel.PersonSender.Person_ID > 0)
                {
                    selectPostcardEntity.Sender_ID = null;
                    _ = await _dbIdentityContext.SaveChangesAsync();

                    List<PostcardEntity> selectAllEntitiesWithSender = [.. (from p in _dbIdentityContext.PostcardEntity
                                                                        where p.Sender_ID == postcardModel.PersonSender.Person_ID
                                                                        select p)];
                    if (selectAllEntitiesWithSender == null || selectAllEntitiesWithSender.Count == 0)
                    {
                        Person SenderSQL = (from p in _dbIdentityContext.Person
                                            where p.Person_ID == postcardModel.PersonSender.Person_ID
                                            select p).First();
                        _ = _dbIdentityContext.Remove(SenderSQL);
                        _ = await _dbIdentityContext.SaveChangesAsync();
                    }
                }

                Person? newReceiver = null;
                if (postcardModel.HasReceiver)
                {
                    if (postcardModel.PersonReceiver != null)
                    {
                        if (postcardModel.PersonReceiver.Person_ID == 0)
                        {
                            newReceiver = new()
                            {
                                Name = postcardModel.PersonReceiver.Name,
                                City_ID = postcardModel.PersonReceiver.City_ID
                            };
                            _ = _dbIdentityContext.Add(newReceiver);
                        }
                        else
                        {
                            Person ReceiverSQL = await (from p in _dbIdentityContext.Person
                                                        where p.Person_ID == postcardModel.PersonReceiver.Person_ID
                                                        select p).FirstAsync();
                            ReceiverSQL.Name = postcardModel.PersonReceiver.Name;
                            ReceiverSQL.City_ID = postcardModel.PersonReceiver.City_ID;

                            if (postcardModel.PersonReceiver.City_ID > 0)
                            {
                                City selectCity = (from c in _dbIdentityContext.City
                                                   where c.City_ID == postcardModel.PersonReceiver.City_ID
                                                   select c).First();
                                ReceiverSQL.City = selectCity;
                            }

                            newReceiver = ReceiverSQL;
                        }
                        _ = await _dbIdentityContext.SaveChangesAsync();
                    }
                }
                else if (postcardModel.PersonReceiver.Person_ID > 0)
                {
                    selectPostcardEntity.Receiver_ID = null;
                    _ = await _dbIdentityContext.SaveChangesAsync();


                    List<PostcardEntity> selectAllEntitiesWithReceiver = [.. (from p in _dbIdentityContext.PostcardEntity
                                                                          where p.Sender_ID == postcardModel.PersonReceiver.Person_ID
                                                                          select p)];
                    if (selectAllEntitiesWithReceiver == null || selectAllEntitiesWithReceiver.Count == 0)
                    {
                        Person ReceiverSQL = await (from p in _dbIdentityContext.Person
                                                    where p.Person_ID == postcardModel.PersonReceiver.Person_ID
                                                    select p).FirstAsync();
                        _ = _dbIdentityContext.Remove(ReceiverSQL);
                        _ = await _dbIdentityContext.SaveChangesAsync();
                    }
                }

                selectPostcardEntity.FilingLocation = postcardModel.PostcardEntity.FilingLocation;
                selectPostcardEntity.Charge = postcardModel.PostcardEntity.Charge;
                selectPostcardEntity.DateInText = postcardModel.PostcardEntity.DateInText;
                selectPostcardEntity.MaterialInt = (int)postcardModel.PostcardEntity.MaterialEnum;
                selectPostcardEntity.ConditionInt = (int)postcardModel.PostcardEntity.ConditionEnum;
                selectPostcardEntity.FilingLocation = postcardModel.PostcardEntity.FilingLocation;
                selectPostcardEntity.Text = postcardModel.PostcardEntity.Text;
                decimal? priceDecimal = null;
                if (!string.IsNullOrEmpty(postcardModel.PriceString))
                {
                    priceDecimal = decimal.Parse(postcardModel.PriceString);
                }

                selectPostcardEntity.Price = priceDecimal;

                if (newSender != null && newSender.Person_ID != 0)
                {
                    selectPostcardEntity.Sender_ID = newSender.Person_ID;
                }

                if (newReceiver != null && newReceiver.Person_ID != 0)
                {
                    selectPostcardEntity.Receiver_ID = newReceiver.Person_ID;
                }

                if (!string.IsNullOrEmpty(postcardModel.ColorRALWriting))
                {
                    selectPostcardEntity.ColorRALWritingFrontside = int.Parse(postcardModel.ColorRALWriting[1..], System.Globalization.NumberStyles.HexNumber);
                }

                if (!string.IsNullOrEmpty(postcardModel.ColorRALPrinting))
                {
                    selectPostcardEntity.ColorRALPrintingBackside = int.Parse(postcardModel.ColorRALPrinting[1..], System.Globalization.NumberStyles.HexNumber);
                }

                _ = await _dbIdentityContext.SaveChangesAsync();

                List<int> PepcIDsBeginningList = [.. (from pepc in _dbIdentityContext.PostcardEntityNManufactoryNCity
                                                  where pepc.PostcardEntity_ID == selectPostcardEntity.PostcardEntity_ID
                                                  select pepc.PostcardEntityNManufactoryNCity_ID)];
                List<int> PepcIDsInsertList = [];
                foreach (string? publisher in postcardModel.ManufactoryIDCityIDList)
                {
                    if (publisher is not null && publisher.Contains("§§"))
                    {
                        string[] splittedPublisher = publisher.Split("§§");
                        if (string.IsNullOrEmpty(splittedPublisher[0]))
                        {
                            continue;
                        }

                        int? cityId = null;
                        IQueryable<PostcardEntityNManufactoryNCity> selectPEPC = from pepc in _dbIdentityContext.PostcardEntityNManufactoryNCity
                                                                                 where pepc.PostcardEntity_ID == selectPostcardEntity.PostcardEntity_ID
                                                                                 && pepc.Publisher_ID == short.Parse(splittedPublisher[0])
                                                                                 select pepc;
                        if (!string.IsNullOrEmpty(splittedPublisher[1]) && short.Parse(splittedPublisher[1]) > 0)
                        {
                            cityId = short.Parse(splittedPublisher[1]);
                            selectPEPC = selectPEPC.Where(p => p.City_ID == cityId);
                        }
                        PostcardEntityNManufactoryNCity? postcardEntityNManufactoryNCity = selectPEPC.FirstOrDefault();

                        if (postcardEntityNManufactoryNCity == null)
                        {
                            PostcardEntityNManufactoryNCity newPostcardEntityPublishrCity = new()
                            {
                                PostcardEntity_ID = selectPostcardEntity.PostcardEntity_ID,
                                Publisher_ID = short.Parse(splittedPublisher[0]),
                                City_ID = cityId
                            };
                            _ = _dbIdentityContext.Add(newPostcardEntityPublishrCity);

                            postcardEntityNManufactoryNCity = newPostcardEntityPublishrCity;
                            _ = await _dbIdentityContext.SaveChangesAsync();
                        }

                        if (postcardEntityNManufactoryNCity != null)
                        {
                            PepcIDsInsertList.Add(postcardEntityNManufactoryNCity.PostcardEntityNManufactoryNCity_ID);
                        }
                    }
                }
                foreach (int beginnId in PepcIDsBeginningList)
                {
                    if (!PepcIDsInsertList.Contains(beginnId))
                    {
                        PostcardEntityNManufactoryNCity pepcToDelete = (from pepc in _dbIdentityContext.PostcardEntityNManufactoryNCity
                                                                        where pepc.PostcardEntityNManufactoryNCity_ID == beginnId
                                                                        select pepc).First() ?? throw new NullReferenceException();
                        _ = _dbIdentityContext.PostcardEntityNManufactoryNCity.Remove(pepcToDelete);
                    }
                }
                _ = await _dbIdentityContext.SaveChangesAsync();

                string currentFileName = string.Empty;
                if (Frontside != null)
                {
                    currentFileName = PicturePreprocess.SaveFileForAnalysis(Frontside, _hostEnvironment);
                    _ = await CreateProductPicture(currentFileName, true, selectPostcardEntity.PostcardEntity_ID, user, true);
                    statusMessage = "Bitte leeren Sie Ihren Cache, damit die Änderung sichtbar wird";
                }

                if (Backside != null)
                {
                    currentFileName = PicturePreprocess.SaveFileForAnalysis(Backside, _hostEnvironment);
                    _ = await CreateProductPicture(currentFileName, false, selectPostcardEntity.PostcardEntity_ID, user, true);
                    statusMessage = "Bitte leeren Sie Ihren Cache, damit die Änderung sichtbar wird";
                }

                transactionScope.Complete();
            }
            catch (TransactionAbortedException ex)
            {
                // DeletePictures
                _logger.LogError("EditPostcardSubmit abgebrochen mit Exception {ex.Message}", ex);
                statusMessage = "Erstellung wurde abgebrochen. Fehler: " + ex.Message;
            }

            return RedirectToAction("AdministerCollectionPostcard", "PostcardDatabase", new { statusMessage });
        } // End EditPostcardSubmit

        public async Task<IActionResult> DeletePostcard(int potentialId, int entityId)
        {
            int PostcardEntityCount = (from pp in _dbIdentityContext.PostcardPotential
                                       join pe in _dbIdentityContext.PostcardEntity
                                       on pp.PostcardPotential_ID equals pe.PostcardPotential_ID
                                       where pp.PostcardPotential_ID == potentialId
                                       select pe).Count();
            if (PostcardEntityCount > 0)
            {
                try
                {
                    using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                    PostcardModel PostcardsSelect = (from p in _dbIdentityContext.PostcardPotential
                                                     join e in _dbIdentityContext.PostcardEntity on p.PostcardPotential_ID equals e.PostcardPotential_ID
                                                     join i in _dbIdentityContext.PostcardImprint on p.PostcardImprint_ID equals i.Image_ID
                                                     where e.PostcardEntity_ID == entityId
                                                     select new PostcardModel
                                                     {
                                                         PostcardPotential = p,
                                                         PostcardEntity = e,
                                                         PostcardImprint = i,
                                                         ProductPictureList = (from Scan in _dbIdentityContext.ProductPicture
                                                                             join pe in _dbIdentityContext.PostcardEntity
                                                                             on Scan.PostcardEntity_ID equals pe.PostcardEntity_ID
                                                                             where pe.PostcardEntity_ID == e.PostcardEntity_ID
                                                                             select Scan).ToList()
                                                     })
                                           .First() ?? throw new NullReferenceException("PostcardsSelect");

                    foreach (ProductPicture scan in PostcardsSelect.ProductPictureList)
                    {
                        if (scan.Frontside)
                        {
                            try
                            {
                                System.IO.File.Delete("wwwroot/images/Klein/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                                System.IO.File.Delete("wwwroot/images/Thumbnail/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                                System.IO.File.Delete("wwwroot/images/Normal/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                            }
                            catch
                            {
                                _logger.LogError("DeletePostcardEntity: ProductPicture nicht gefunden: {scan.ProductPicture_Id}.{scan.Pictures_Format}", scan.ProductPicture_ID, scan.FileExtension);
                            }
                        }
                        else
                        {
                            System.IO.File.Delete("wwwroot/images/Normal/" + scan.ProductPicture_ID + "." + scan.FileExtension);
                        }
                        _ = _dbIdentityContext.Remove(scan);
                        _ = await _dbIdentityContext.SaveChangesAsync();
                    }

                    _ = _dbIdentityContext.Remove(PostcardsSelect.PostcardPotential);
                    _ = await _dbIdentityContext.SaveChangesAsync();
                    _ = _dbIdentityContext.Remove(PostcardsSelect.PostcardImprint);
                    _ = await _dbIdentityContext.SaveChangesAsync();

                    scope.Complete();
                }
                catch (TransactionAbortedException ex)
                {
                    _logger.LogError("DeletePostcard wurde abgebrochen mit Exception {ex}", ex);
                }
            }
            else
            {
                return NotFound("Entität wurde nicht gefunden");
            }

            return RedirectToAction("AdministerCollectionPostcard", "PostcardDatabase");
        }

        public async Task<IActionResult> DownloadPostcards(string userId, int? potentialId = null)
        {
            string sourceDir = Path.Combine(_hostEnvironment.WebRootPath, Path.Combine("images", "Original"));
            string downloadFolder = Path.Combine(_hostEnvironment.WebRootPath, "Download_" + userId);
            string zipFile = Path.Combine(_hostEnvironment.WebRootPath, "Download_" + userId + ".zip");

            _ = Directory.CreateDirectory(downloadFolder);

            DbActionsPostcard dbChangesPostcard = new(_dbIdentityContext, _userManager, processCity, _logger);
            List<PostcardModel> postcardList = [];
            postcardList = potentialId == null
                ? ([.. dbChangesPostcard.QueryPostcardModel(userId)])
                : ([.. dbChangesPostcard.QueryPostcardModel(userId).Where(x => x.PostcardPotential.PostcardPotential_ID.Equals(potentialId))]);

            foreach (PostcardModel postcard in postcardList)
            {
                foreach (ProductPicture scan in postcard.ProductPictureList)
                {
                    string sourceFilePath = Path.Combine(sourceDir, scan.ProductPicture_ID.ToString() + ".png");
                    string targetFilePath = Path.Combine(downloadFolder, scan.ProductPicture_ID.ToString() + ".png");
                    System.IO.File.Copy(sourceFilePath, targetFilePath, true);
                }
            }

            string yamlFile = Path.Combine(downloadFolder, "PostcardDatas.yaml");
            if (!System.IO.File.Exists(yamlFile))
            {
                // Create a file to write to.
                using FileStream sw = System.IO.File.Create(yamlFile);
                byte[] yaml = [];
                foreach (PostcardModel postcard in postcardList)
                {
                    Sammlerplattform.Models.Download.PostcardDownloadModel postcardDownloadModel = YamlProcessor.ComposeForDownload(postcard, _dbIdentityContext, processCity);
                    byte[] spty = YamlProcessor.SerializePostcardToYaml(postcardDownloadModel);
                    sw.Write(spty);
                }
            }

            ZipFile.CreateFromDirectory(downloadFolder, zipFile);
            MemoryStream memory = new();
            using (FileStream stream = new(zipFile, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            Directory.Delete(downloadFolder, true);
            System.IO.File.Delete(zipFile);

            return File(memory, "application/zip", Path.GetFileName(zipFile));
        }

        public ActionResult AddEra(int sourceId, string source, string statusMessage)
        {
            ViewData["SourceId"] = sourceId;
            ViewData["BackToList"] = source;
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public async Task<ActionResult> AddEraSubmit(Era era, string sourceId, string source)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(AddEra), new { sourceId, source, statusMessage = "Eingaben ungültig." });
            }

            string statusMessage = string.Empty;
            Era? EraSelect = (from e in _dbIdentityContext.Era
                              where e.EraLong == era.EraLong
                              select e).FirstOrDefault();
            if (EraSelect == null)
            {
                Era newEra = new()
                {
                    EraLong = era.EraLong,
                    EraShort = era.EraShort
                };

                _ = _dbIdentityContext.Add(newEra);
                try
                {
                    _ = await _dbIdentityContext.SaveChangesAsync();
                    statusMessage = "Ära wurde erstellt.";
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    _logger.LogError("AddEraSubmit fehlgeschlagen mit Exception: {ex}, EraLong: {era.EraLong}, EraShort: {era.EraShort}",
                        ex, era.EraLong, era.EraShort);
                    statusMessage = "Es ist ein Fehler beim Hinzufügen der Ära augetreten. Der Support wurde benachrichtigt.";
                }
            }
            else
            {
                statusMessage = "Ära existiert bereits.";
            }

            return RedirectToAction(nameof(AddEra), new { sourceId, source, statusMessage });
        }
    }


    [Authorize(Policy = "SubscribedDiskspacePolicy")]
    public class DbActionsPostcard(DbIdentityContext _dbIdentityContext, UserManager<UsingIdentityUser> userManager, IProcessCity processCity, ILogger<PostcardDatabaseController> _logger)
    {
        public IQueryable<PostcardModel> QueryPostcardModel(string userId)
        {
            CityOperationParameterModel cityParameterModel = new();
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises. Cause of Bug https://github.com/dotnet/efcore/issues/17212
            IQueryable<PostcardModel> postcardQuery = from pse in _dbIdentityContext.PostcardEntity
                                                      join pso in _dbIdentityContext.PostcardPotential
                                                          .Include(x => x.CityList).ThenInclude(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                                                          .Include(x => x.CityList).ThenInclude(x => x.PostalcodeICollection)
                                                          .Include(y => y.CityList).ThenInclude(x => x.Geography)
                                                      on pse.PostcardPotential_ID equals pso.PostcardPotential_ID
                                                      join picture in _dbIdentityContext.PostcardImprint
                                                      on pso.PostcardImprint_ID equals picture.Image_ID into LeftOuterPic
                                                      from subPic in LeftOuterPic.DefaultIfEmpty()
                                                      join print in _dbIdentityContext.Printing
                                                      on subPic.Printing_ID equals print.Printing_ID into LeftOuterPrint
                                                      from subPostPrint in LeftOuterPrint.DefaultIfEmpty()
                                                      join perse in _dbIdentityContext.Person
                                                      on pse.Sender_ID equals perse.Person_ID into LeftOuterSender
                                                      from subPostSender in LeftOuterSender.DefaultIfEmpty()
                                                      join perRe in _dbIdentityContext.Person
                                                          .Include(x => x.City).ThenInclude(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                                                          .Include(x => x.City).ThenInclude(x => x.PostalcodeICollection)
                                                          .Include(y => y.City).ThenInclude(x => x.Geography)
                                                      on pse.Receiver_ID equals perRe.Person_ID into LeftOuterReceiver
                                                      from subPostReceiver in LeftOuterReceiver.DefaultIfEmpty()
                                                      join aa in _dbIdentityContext.Person
                                                      on subPic.ArtistAuthor_ID equals aa.Person_ID into LeftOuterAA
                                                      from subPostAA in LeftOuterAA.DefaultIfEmpty()
                                                      join era in _dbIdentityContext.Era
                                                      on subPic.Era_ID equals era.Era_ID into LeftOuterEra
                                                      from subPostEra in LeftOuterEra.DefaultIfEmpty()
                                                      join user in userManager.Users
                                                      on pse.UsingIdentityUsers_ID equals user.Id
                                                      where user.Id == userId
                                                      select new PostcardModel
                                                      {
                                                          PostcardEntity = pse,
                                                          PostcardPotential = pso,
                                                          PostcardImprint = subPic,
                                                          ProductPictureList = (from Scan in _dbIdentityContext.ProductPicture
                                                                              join pe in _dbIdentityContext.PostcardEntity
                                                                              on Scan.PostcardEntity_ID equals pe.PostcardEntity_ID
                                                                              where pe.PostcardEntity_ID == pse.PostcardEntity_ID
                                                                              select Scan).ToList(),
                                                          Printing = subPostPrint,
                                                          PersonSender = subPostSender,
                                                          PersonReceiver = subPostReceiver,
                                                          AuthorArtist = subPostAA,
                                                          Era = subPostEra,
                                                          ManufactoryTupleList = (from p in _dbIdentityContext.Manufactory
                                                                                  join pemc in _dbIdentityContext.PostcardEntityNManufactoryNCity
                                                                                  on p.Manufactory_ID equals pemc.Publisher_ID
                                                                                  join c in _dbIdentityContext.City.Include(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                                                                                  on pemc.City_ID equals c.City_ID into leftOuterCity
                                                                                  from subc in leftOuterCity.DefaultIfEmpty()
                                                                                  where pemc.PostcardEntity_ID == pse.PostcardEntity_ID
                                                                                  select new ValueTuple<Manufactory, City, List<City>>
                                                                                  (
                                                                                      p,
                                                                                      subc,
                                                                                      (from c in _dbIdentityContext.City.Include(m => m.ManufactoryList)
                                                                                       .Include(x => x.Geography)
                                                                                       .Include(x => x.CityNOeconymICollection.Where(y => y.CurrentName)).ThenInclude(x => x.Oeconym)
                                                                                       where c.ManufactoryList.Any(c => c.Manufactory_ID.Equals(p.Manufactory_ID))
                                                                                       select c).ToList()
                                                                                   )).ToList()
                                                      };
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.

            return postcardQuery;
        }

        public void ProcessAnalysisResultParameters(PostcardAnalyzeResultParameters parameters, PostcardModel postcardModel)
        {
            if (parameters.AuthorArtistList.Count > 0)
            {
                foreach (string authorArtist in parameters.AuthorArtistList)
                {
                    if (!string.IsNullOrEmpty(authorArtist))
                    {
                        Person? selectAA = (from aa in _dbIdentityContext.Person
                                            where aa.Name == authorArtist
                                            select aa).FirstOrDefault();
                        if (selectAA is not null)
                        {
                            postcardModel.AuthorArtist = selectAA;
                        }
                        else
                        {
                            Person newAuthorArtist = new()
                            {
                                Name = authorArtist
                            };
                            _ = _dbIdentityContext.Add(newAuthorArtist);
                            try
                            {
                                _ = _dbIdentityContext.SaveChanges();
                            }
                            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                            {
                                _logger.LogError("ProcessAnalysisResultParameters Authorartist abgebrochen mit Exception {ex}, Name {authorArtist}.",
                                    ex, authorArtist);
                            }

                            postcardModel.AuthorArtist = newAuthorArtist;
                        }

                        // There should be only one Author/Artist
                        break;
                    }
                }
                postcardModel.HasImage = true;
            }

            if (parameters.BuildingList.Count > 0)
            {
                foreach (string building in parameters.BuildingList)
                {
                    if (!string.IsNullOrEmpty(building))
                    {
                        postcardModel.PostcardImprint.Buildings += building.ToString() + ";";
                    }
                }
                postcardModel.HasImage = true;
            }

            if (parameters.TextList.Count > 0)
            {
                foreach (string text in parameters.TextList)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        postcardModel.PostcardEntity.Text += text + " ";
                    }
                }
            }

            if (parameters.CityList.Count > 0)
            {
                List<(string City, string geographyOfCity)> cities = [];
                foreach (string? city in parameters.CityList)
                {
                    if (city is not null && city.Contains("§§"))
                    {
                        string[] splittedCity = city.Split("§§");
                        cities.Add((splittedCity[0], splittedCity[1]));
                    }
                }

                foreach ((string City, string GeographyOfCity) in cities)
                {
                    if (!string.IsNullOrEmpty(City))
                    {
                        CityOperationParameterModel cityParameterModel = new()
                        {
                            Oeconym = new Oeconym() { OeconymName = City },
                            Geography = new Geography() { GeographyName = GeographyOfCity }
                        };
                        City? selectCity = processCity.GetCityWithPredicates(processCity.CityParametersOperationToSearch(cityParameterModel)).FirstOrDefault();
                        if (selectCity == null)
                        {
                            (City city, int statuscode, string message) = processCity.CreateCity(cityParameterModel);
                            if (statuscode != 201)
                            {
                                _logger.LogError("{message}", message);
                            }
                            else
                            {
                                postcardModel.CityTupleList.Add((city, city.PostalcodeICollection.ToList(), city.Geography!));
                            }
                        }
                        else
                        {
                            postcardModel.CityTupleList.Add((selectCity, [.. selectCity.PostalcodeICollection], new()));
                        }
                    }
                }
                postcardModel.HasImage = true;
            }

            // TODOsammlerdb: Orte werden nicht erstellt, und falls zugeordnet(z.B. Berlin zu Verlag der keine Ort hat)
            if (parameters.PublisherList.Count > 0)
            {
                List<(string name, string city, string geography)> publishers = [];
                foreach (string? publisher in parameters.PublisherList)
                {
                    if (publisher is not null && publisher.Contains("§§"))
                    {
                        string[] splittedPublisher = publisher.Split("§§");
                        publishers.Add((splittedPublisher[0], splittedPublisher[1], splittedPublisher[2]));
                    }
                }

                foreach ((string name, string city, string geography) in publishers)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    CityOperationParameterModel cityParameterModel = new()
                    {
                        Oeconym = new Oeconym() { OeconymName = city },
                        Geography = new Geography() { GeographyName = geography }
                    };
                    City? selectedCity = processCity.GetCityWithPredicates(processCity.CityParametersOperationToSearch(cityParameterModel)).FirstOrDefault();

                    selectedCity ??= processCity.CreateCity(cityParameterModel).city;

                    Manufactory? selectPublisher = (from p in _dbIdentityContext.Manufactory
                                                    where p.ManufactoryName.Equals(name)
                                                    select p).FirstOrDefault();

                    if (selectPublisher == null)
                    {
                        Manufactory newPublisher = new()
                        {
                            ManufactoryName = name
                        };
                        if (selectedCity != null)
                        {
                            newPublisher.CityICollection ??= [];
                            newPublisher.CityICollection.Add(selectedCity);
                        }
                        _ = _dbIdentityContext.Add(newPublisher);
                        try
                        {
                            _ = _dbIdentityContext.SaveChanges();
                        }
                        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                        {
                            _logger.LogError("ProcessAnalysisResultParameters publisher abgebrochen mit Exception {ex}, Name {name}.",
                                    ex, name);
                        }

                        if (selectedCity == null)
                        {
                            postcardModel.ManufactoryTupleList.Add((newPublisher, new(), []));
                        }
                        else
                        {
                            postcardModel.ManufactoryTupleList.Add((newPublisher, selectedCity, []));
                        }
                    }
                    else
                    {
                        selectPublisher.CityICollection ??= [];
                        selectPublisher.CityICollection.Add(selectedCity);
                        _ = _dbIdentityContext.SaveChanges();
                        postcardModel.ManufactoryTupleList.Add((selectPublisher, selectedCity, []));
                    }
                }
            }

            // TODOsammlerdb: Person wird nicht gespeichert
            if (parameters.AddressList.Count > 0)
            {
                List<(string name, string PLZ, string City)> addressTupleList = [];
                foreach (string? address in parameters.AddressList)
                {
                    if (address is not null && address.Contains("§§"))
                    {
                        string[] splittedAddresses = address.Split("§§");
                        addressTupleList.Add((splittedAddresses[0] + splittedAddresses[1], string.Empty, splittedAddresses[4]));
                    }
                }

                foreach ((string name, string PLZ, string City) address in addressTupleList)
                {
                    City? selectCity = (from c in _dbIdentityContext.City.Include(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                                        where c.CityNOeconymICollection.Any(x => x.Oeconym.OeconymName.Equals(address.City))
                                        select c).FirstOrDefault();

                    IQueryable<Person> selectPerson = from p in _dbIdentityContext.Person.Include(x => x.City)
                                                      where p.Name != null && p.Name!.Equals(address.name)
                                                      select p;
                    if (selectCity != null)
                    {
                        selectPerson = selectPerson.Where(x => x.City != null && x.City.City_ID.Equals(selectCity.City_ID));
                    }

                    List<Person> personCityTupleList = [.. selectPerson];

                    if (personCityTupleList.Count > 0)
                    {
                        //
                        int maxAmount = 0;
                        if (!string.IsNullOrEmpty(address.City))
                        {
                            maxAmount++;
                        }

                        if (maxAmount > 0)
                        {
                            Dictionary<int, int> personFit = [];
                            foreach (Person? p in selectPerson)
                            {
                                int amount = 0;
                                if (!string.IsNullOrEmpty(address.City) && p.City != null && p.City.CityNOeconymICollection.Any(x => x.Oeconym.OeconymName.Equals(address.City)))
                                {
                                    amount++;
                                }

                                personFit.Add(p.Person_ID, amount);
                            }
                            int personID = personFit.MaxBy(x => x.Value).Key;
                            int maxAmountSelected = personFit.MaxBy(x => x.Value).Value;

                            if (maxAmountSelected > 0)
                            {
                                if (maxAmountSelected > maxAmount)
                                {
                                    Console.WriteLine("Fehler CreatePostcard adress maxAmountSelected>maxAmount");
                                }
                                else if (maxAmount > maxAmountSelected)
                                {
                                    postcardModel.PersonReceiverTuple = CreatePerson(address);
                                }
                                else
                                {
                                    Person? anonymousPersonSelect = selectPerson.FirstOrDefault(x => x.Person_ID.Equals(personID));
                                    if (anonymousPersonSelect != default)
                                    {
                                        postcardModel.PersonReceiverTuple = (anonymousPersonSelect, anonymousPersonSelect.City);
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        postcardModel.PersonReceiverTuple = CreatePerson(address);
                    }
                }
                postcardModel.HasReceiver = true;
            }
        }

        public (Person, City?) CreatePerson((string name, string postalcode, string city) address)
        {
            Person newPerson = new()
            {
                Name = address.name
            };
            City city = new();

            if (!string.IsNullOrEmpty(address.city))
            {
                CityOperationParameterModel cityParameterModel = new()
                {
                    Oeconym = new Oeconym() { OeconymName = address.city },
                    Postalcode = new Postalcode() { PostalcodeNumber = address.postalcode }
                };
                City? selectedCity = processCity.GetCityWithPredicates(processCity.CityParametersOperationToSearch(cityParameterModel)).FirstOrDefault();
                selectedCity ??= processCity.CreateCity(cityParameterModel).city;
                city = selectedCity;

                newPerson.City_ID = city.City_ID;
                newPerson.City = city;
                _ = _dbIdentityContext.SaveChanges();
            }

            return (newPerson, city);
        }
    }
}
