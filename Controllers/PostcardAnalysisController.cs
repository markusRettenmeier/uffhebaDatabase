using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sammlerplattform.Controllers.PictureAnaylsis;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using System.Data.Entity;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Sammlerplattform.Controllers
{
    [Authorize(Policy = "SubscribedAnalysisToolPolicy")]
    public partial class PostcardAnalysisController(IWebHostEnvironment hostEnvironment, UserManager<UsingIdentityUser> userManager,
        DbIdentityContext dbIdentityContext, ILogger<AccountController> logger, ILogger<PostcardDatabaseController> logger2, IProcessCity processCity) : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
        private readonly UserManager<UsingIdentityUser> _userManager = userManager;
        private readonly DbIdentityContext _dbIdentityContext = dbIdentityContext;
        private readonly ILogger<AccountController> _logger = logger;

        public ActionResult UploadPictures()
        {
            return View();
        }

        public ActionResult AnalyzeImage(IFormFile fileFrontside, IFormFile fileBackside)
        {
            string userId = _userManager.GetUserId(User) ?? throw new NullReferenceException("user");
            IdentityUserClaim<string> claim = (from u in _dbIdentityContext.UserClaims
                                               where u.UserId == userId
                                               && u.ClaimType == "SubscribedAnalysisTool"
                                               select u).First();
            if (claim.ClaimValue != null)
            {
                PaymentService.SendUsageRecordAnalysisTool(claim.ClaimValue, 2, _logger);
            }

            string dir = Path.Combine(_hostEnvironment.WebRootPath, "DetectedTexts_AnaylzeImage");
            string dirTextFiles = Path.Combine(dir, "TextFiles");

            WordCategorizationModel wordCategorizationModel = new()
            {
                Cities =
                [
                    .. (from c in _dbIdentityContext.Oeconym
                        select c.OeconymName),
                ],
                Publishers =
                [
                    .. (from p in _dbIdentityContext.Manufactory
                        select p.ManufactoryName),
                ],
                AuthorArtists =
                [
                    .. (from aa in _dbIdentityContext.Person.Include(x => x.ProfessionICollection)
                        where aa.ProfessionICollection.Any(x => x.Name.Equals(1) || x.Name.Equals(2))
                        select aa.Name),
                ]
            };
            List<string> buildingList = [];
            List<(string cityName, string geographyOfCity)> cityTupleList = [];
            List<string> authorArtistList = [];
            List<(string publisherName, string publisherCity, string geographyOfCity)> publisherTupleList = [];
            List<string> yearList = [];
            List<string> dateList = [];
            List<string> numberList = [];
            List<string> forenameList = [];
            List<string> surnameList = [];
            List<string> nameList = [];
            List<string> geographyList = [];
            List<string> occasionList = [];
            List<string> streetList = [];
            List<(string foreName, string surName, string street, string Streetnumber, string City)> addressTupleList = [];
            // Werden noch nicht benutzt, da Ergebnis des OCR zu schlecht
            List<(string postmarkCity, string postmarkGeographyOfCity, string postmarkDate, string postmarkText)> postmarkTupleList = [];
            string stamp = string.Empty;
            string text = string.Empty;

            wordCategorizationModel.Frontside = true;
            PicturePreprocess.AnalyzeImage(fileFrontside, _hostEnvironment, wordCategorizationModel);
            wordCategorizationModel.Frontside = false;
            PicturePreprocess.AnalyzeImage(fileBackside, _hostEnvironment, wordCategorizationModel);

            ////Bilder abspeichern
            string pathFrontside = PicturePreprocess.SaveFileForAnalysis(fileFrontside, _hostEnvironment);
            string pathBackside = PicturePreprocess.SaveFileForAnalysis(fileBackside, _hostEnvironment);

            if (wordCategorizationModel.WordBlockCategorization.Count > 0)
            {
                int idx = 0;
                List<int> cityBlockIdList = [];
                List<int> publisherBlockIdList = [];
                List<int> authorArtistBlockIdList = [];
                List<int> addressBlockIdList = [];
                List<int> forenameBlockIdList = [];
                List<int> surnameBlockIdList = [];
                List<int> postmarkBlockIdList = [];

                foreach ((string content, string category, double prob, bool Frontside) in wordCategorizationModel.Blocks)
                {
                    switch (category)
                    {
                        case "Building":
                            buildingList.Add(content);
                            break;
                        case "Publisher":
                            publisherBlockIdList.Add(idx);
                            break;
                        case "City":
                            cityBlockIdList.Add(idx);
                            break;
                        case "AuthorArtist":
                            authorArtistBlockIdList.Add(idx);
                            break;
                        case "Address":
                            addressBlockIdList.Add(idx);
                            break;
                        case "Forename":
                            forenameBlockIdList.Add(idx);
                            text += " " + content;
                            break;
                        case "Surname":
                            surnameBlockIdList.Add(idx);
                            text += " " + content;
                            break;
                        case "Postmark":
                            postmarkBlockIdList.Add(idx);
                            break;
                        case "Stamp":
                            stamp += " " + content;
                            break;
                        case "Text":
                            text += " " + content;
                            break;
                        case "Date":
                            dateList.Add(content);
                            break;
                        case "Year":
                            yearList.Add(content);
                            break;
                        case "Number":
                            numberList.Add(content);
                            break;
                        case "Geography":
                            geographyList.Add(content);
                            break;
                        case "Occasion":
                            occasionList.Add(content);
                            break;
                        case "Straßenzug":
                            streetList.Add(content);
                            break;
                    }

                    idx++;
                }

                // It can be, that publisher is categorized only as Author / Artist, and so publisher is msising
                if (publisherBlockIdList.Count == 0)
                {
                    publisherBlockIdList.AddRange(authorArtistBlockIdList);
                }

                string publisherName = string.Empty;
                string publisherCityName = string.Empty;
                string publisherGeographyName = string.Empty;
                string addressForeName = string.Empty;
                string addressSurName = string.Empty;
                string addressStreet = string.Empty;
                string addressNumber = string.Empty;
                string addressCity = string.Empty;
                string postmarkCity = string.Empty;
                string postmarkGeographyOfCity = string.Empty;
                string postmarkDate = string.Empty;
                string postmarkText = string.Empty;
                bool isSurName = false;
                bool isNameComplete = false;
                string nameString = string.Empty;
                string bindingWord = string.Empty;
                string cityBlockText = string.Empty;
                int currentBlock = 0;

                if (wordCategorizationModel.WordBlockCategorization is not null && wordCategorizationModel.WordBlockCategorization.Count > 0)
                {
                    foreach ((string Word, int Block, int Position, List<(double Probability, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) in wordCategorizationModel.WordBlockCategorization)
                    {
                        (double Probability, string CategoryName, string? CategorizedTo, string CategorizedWhere) maxCategory = new();
                        if (Category.Count > 0)
                        {
                            maxCategory = Category.MaxBy(x => x.Probability);
                        }

                        if (currentBlock != Block)
                        {
                            //cityIn = false;
                            isSurName = false;
                            isNameComplete = false;
                            if (!string.IsNullOrEmpty(cityBlockText))
                            {
                                text += " " + cityBlockText;
                                cityBlockText = string.Empty;
                                bindingWord = string.Empty;
                            }
                            else if (!string.IsNullOrEmpty(publisherName))
                            {
                                publisherTupleList.Add((publisherName, publisherCityName, publisherGeographyName));
                                publisherName = string.Empty;
                                publisherCityName = string.Empty;
                                publisherGeographyName = string.Empty;
                            }
                            else if (!string.IsNullOrEmpty(addressForeName) || !string.IsNullOrEmpty(addressSurName) || !string.IsNullOrEmpty(addressStreet) || !string.IsNullOrEmpty(addressNumber) || !string.IsNullOrEmpty(addressCity))
                            {
                                addressTupleList.Add((addressForeName, addressSurName, addressStreet, addressNumber, addressCity));
                                addressForeName = string.Empty;
                                addressSurName = string.Empty;
                                addressStreet = string.Empty;
                                addressNumber = string.Empty;
                                addressCity = string.Empty;
                                bindingWord = string.Empty;
                            }
                            else if (!string.IsNullOrEmpty(postmarkCity) || !string.IsNullOrEmpty(postmarkGeographyOfCity) || !string.IsNullOrEmpty(postmarkDate) || !string.IsNullOrEmpty(postmarkText))
                            {
                                postmarkTupleList.Add((postmarkCity, postmarkGeographyOfCity, postmarkDate, postmarkText));
                                postmarkCity = string.Empty;
                                postmarkGeographyOfCity = string.Empty;
                                postmarkDate = string.Empty;
                                postmarkText = string.Empty;
                                bindingWord = string.Empty;
                            }
                            nameList.Add(nameString);
                            nameString = string.Empty;
                            currentBlock = Block;
                        }

                        // Aufteilung des Verlages und des Ortes
                        if (publisherBlockIdList.Contains(Block))
                        {
                            if (Category.Exists(x => x.CategoryName.Equals("City") && x.Probability >= 0.8))
                            {
                                publisherCityName = Word;
                            }
                            else if (Category.Exists(x => x.CategoryName.Equals("Geography") && x.Probability >= 0.8))
                            {
                                publisherGeographyName = Word;
                            }
                            else if (string.IsNullOrEmpty(publisherName))
                            {
                                if (!RegexSpecialCharacter().IsMatch(Word) && !Word.Equals("Verlag", StringComparison.OrdinalIgnoreCase))
                                {
                                    publisherName = Word;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (RegexBeginningOfWord().IsMatch(Word))
                                {
                                    publisherName += " " + Word;
                                }
                                else if (RegexConjunctionWord().IsMatch(Word))
                                {
                                    publisherName += Word;
                                }
                            }

                            if (authorArtistBlockIdList.Contains(Block))
                            {
                                if (maxCategory.CategoryName != null && maxCategory.CategoryName.Equals("Surname") && maxCategory.Probability >= 0.5)
                                {
                                    nameString += " " + Word;
                                }
                                else if (Word is "von" or "v." or "-")
                                {
                                    nameString += Word + " " + nameString;
                                }
                                else if (maxCategory.CategoryName != null && maxCategory.CategoryName.Equals("Forename") && maxCategory.Probability >= 0.5)
                                {
                                    if (nameString.Contains(','))
                                    {
                                        nameString += " " + Word;
                                    }
                                    else
                                    {
                                        nameString += ", " + Word;
                                    }

                                    isSurName = true;
                                }
                                else
                                {
                                    authorArtistList.Add(nameString);
                                    nameString = string.Empty;
                                }
                            }
                        }
                        else if (authorArtistBlockIdList.Contains(Block))
                        {
                            if (maxCategory.CategoryName != null && maxCategory.CategoryName.Equals("Surname") && maxCategory.Probability >= 0.5)
                            {
                                nameString += " " + Word;
                            }
                            else if (Word is "von" or "v." or "-")
                            {
                                nameString += Word + " " + nameString;
                            }
                            else if (maxCategory.CategoryName != null && maxCategory.CategoryName.Equals("Forename") && maxCategory.Probability >= 0.5)
                            {
                                if (nameString.Contains(','))
                                {
                                    nameString += " " + Word;
                                }
                                else
                                {
                                    nameString += ", " + Word;
                                }

                                isSurName = true;
                            }
                            else
                            {
                                authorArtistList.Add(nameString);
                                nameString = string.Empty;
                            }
                        }
                        //Aufteilung des Ortes und zugehörigen Landschaft (z.B. Fluss, See oder Berg)
                        else if (cityBlockIdList.Contains(Block))
                        {
                            if (Category.Exists(x => x.CategoryName.Equals("City") && x.Probability >= 0.5))
                            {
                                cityBlockText += bindingWord + Word;
                            }
                            else if (RegexConjunctionWord().IsMatch(Word))
                            {
                                bindingWord = Word;
                            }
                            else if (Category.Exists(x => x.CategoryName.Equals("Geography") && x.Probability >= 0.5) || Category.Exists(x => x.CategoryName.Equals("Building") && x.Probability >= 0.5) || Category.Exists(x => x.CategoryName.Equals("Straßenzug") && x.Probability >= 0.5))
                            {
                                if (Category.Exists(x => x.CategoryName.Equals("Geography") && x.Probability >= 0.5))
                                {
                                    if (cityTupleList.Exists(x => x.cityName.Equals(cityBlockText) && string.IsNullOrEmpty(x.geographyOfCity)))
                                    {
                                        int posToRemove = cityTupleList.FindIndex(x => x.cityName.Equals(cityBlockText));
                                        cityTupleList.RemoveAt(posToRemove);
                                    }
                                    if (!string.IsNullOrEmpty(cityBlockText))
                                    {
                                        cityTupleList.Add((cityBlockText, Word));
                                    }

                                    cityBlockText = string.Empty;
                                }
                                else if (Category.Exists(x => x.CategoryName.Equals("Building") && x.Probability >= 0.5))
                                {
                                    if (!string.IsNullOrEmpty(cityBlockText))
                                    {
                                        cityTupleList.Add((cityBlockText, string.Empty));
                                    }

                                    buildingList.Add(Word);
                                    cityBlockText = string.Empty;
                                }
                                else if (Category.Exists(x => x.CategoryName.Equals("Straßenzug") && x.Probability >= 0.5))
                                {
                                    if (!string.IsNullOrEmpty(cityBlockText))
                                    {
                                        cityTupleList.Add((cityBlockText, string.Empty));
                                    }

                                    streetList.Add(Word);
                                    cityBlockText = string.Empty;
                                }
                            }
                            else
                            {
                                text += Word;
                                if (!string.IsNullOrEmpty(cityBlockText) && !cityTupleList.Exists(x => x.cityName.Equals(cityBlockText)))
                                {
                                    cityTupleList.Add((cityBlockText, string.Empty));
                                    cityBlockText = string.Empty;
                                }
                            }
                        }
                        else if (addressBlockIdList.Contains(Block))
                        {
                            if (Category.Count > 0)
                            {
                                //var maxCategory = Category.MaxBy(x => x.Probability);
                                if (maxCategory.CategoryName != null && maxCategory.CategoryName.Equals("Address") && maxCategory.Probability >= 1)
                                {
                                    //Salutation is not treated
                                }
                                else if (!isSurName && !Category.Exists(x => x.CategoryName.Equals("Surname") && x.Probability >= 0.5))
                                {
                                    if (string.IsNullOrEmpty(addressForeName))
                                    {
                                        addressForeName = Word;
                                    }
                                    else
                                    {
                                        addressForeName += " " + Word;
                                    }
                                }
                                else if (!isSurName && Word is "von" or "v." or "-" or "und" or "zu")
                                {
                                    addressSurName = Word;
                                }
                                else if (Category.Exists(x => x.CategoryName.Equals("Surname") && x.Probability >= 0.5))
                                {
                                    if (string.IsNullOrEmpty(addressSurName))
                                    {
                                        addressSurName = Word;
                                    }
                                    else
                                    {
                                        addressSurName += " " + Word;
                                    }

                                    isSurName = true;
                                }
                                else if (Category.Exists(x => x.CategoryName.Equals("Straßenzug") && x.Probability >= 0.5))
                                {
                                    if (string.IsNullOrEmpty(addressStreet))
                                    {
                                        addressStreet = Word;
                                    }
                                    else
                                    {
                                        addressStreet += " " + Word;
                                    }
                                }
                                else if (Category.Exists(x => x.CategoryName.Equals("Number") && x.Probability >= 0.5))
                                {
                                    addressNumber = Word;
                                }
                                else if (Category.Exists(x => x.CategoryName.Equals("City") && x.Probability >= 0.5))
                                {
                                    addressCity += bindingWord + Word;
                                }
                                else if (RegexConjunctionWord().IsMatch(Word))
                                {
                                    bindingWord = Word;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(addressStreet))
                                    {
                                        addressStreet = Word;
                                    }
                                    else
                                    {
                                        addressStreet += " " + Word;
                                    }
                                }
                            }
                        }
                        else if (postmarkBlockIdList.Contains(Block))
                        {
                            if (Category.Exists(x => x.CategoryName.Equals("Geography") && x.Probability >= 0.5))
                            {
                                postmarkGeographyOfCity = Word;
                            }
                            else if (Category.Exists(x => x.CategoryName.Equals("Date") && x.Probability >= 0.5))
                            {
                                postmarkDate = Word;
                            }
                            else if (Category.Exists(x => x.CategoryName.Equals("City") && x.Probability >= 0.5))
                            {
                                postmarkCity += bindingWord + Word;
                            }
                            else if (RegexConjunctionWord().IsMatch(Word))
                            {
                                bindingWord = Word;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(postmarkText))
                                {
                                    postmarkText = Word;
                                }
                                else
                                {
                                    postmarkText += " " + Word;
                                }
                            }
                        }
                        else if (!isNameComplete && (forenameBlockIdList.Contains(Block) || surnameBlockIdList.Contains(Block)))
                        {
                            if (!isSurName && Category.Exists(x => x.CategoryName.Equals("Forename") && x.Probability >= 0.5))
                            {
                                nameString += " " + Word;
                            }
                            else if (Word is "von" or "v." or "-")
                            {
                                nameString += " " + Word;
                            }
                            else if (Category.Exists(x => x.CategoryName.Equals("Surname") && x.Probability >= 0.5))
                            {
                                nameString += " " + Word;
                                isSurName = true;
                            }
                            else if (Category.Exists(x => x.CategoryName.Equals("Forename") && x.Probability >= 0.5))
                            {
                                nameList.Add(nameString);
                                nameString = Word;
                            }
                        }
                    }
                }
            }

            //Tuples can't be sent to IActionResult
            List<string> cityList = [];
            foreach ((string cityName, string geographyOfCity) in cityTupleList)
            {
                cityList.Add(cityName + "§§" + geographyOfCity);
            }
            List<string> publisherList = [];
            foreach ((string publisherName, string publisherCity, string publisherGeography) in publisherTupleList)
            {
                publisherList.Add(publisherName + "§§" + publisherCity + "§§" + publisherGeography);
            }
            List<string> addressList = [];
            foreach ((string foreName, string surName, string street, string Streetnumber, string City) in addressTupleList)
            {
                addressList.Add(foreName + "§§" + surName + "§§" + street + "§§" + Streetnumber + "§§" + City);
            }
            List<string> postmarkList = [];
            foreach ((string postmarkCity, string postmarkGeographyOfCity, string postmarkDate, string postmarkText) in postmarkTupleList)
            {
                postmarkList.Add(postmarkCity + "§§" + postmarkGeographyOfCity + "§§" + postmarkDate + "§§" + postmarkText);
            }

            return RedirectToAction(nameof(AnalysisResult), new
            {
                pathFrontside,
                pathBackside,
                buildingList,
                cityList,
                authorArtistList,
                publisherList,
                addressList,
                postmarkList,
                stamp,
                text,
                dateList,
                yearList,
                numberList,
                nameList,
                geographyList,
                occasionList,
                streetList
            });
        }

        public IActionResult AnalysisResult(string pathFrontside, string pathBackside, List<string> buildingList, List<string> cityList, List<string> authorArtistList
            , List<string> publisherList, List<string> addressList, List<string> postmarkList, string stamp, string text, List<string> dateList, List<string> yearList
            , List<string> numberList, List<string> nameList, List<string> geographyList, List<string> occasionList, List<string> streetList)
        {
            ViewData["PathFrontside"] = pathFrontside;
            ViewData["PathBackside"] = pathBackside;
            ViewData["Building"] = buildingList;
            ViewData["Publisher"] = publisherList;
            ViewData["City"] = cityList;
            ViewData["AuthorArtist"] = authorArtistList;
            ViewData["Address"] = addressList;
            ViewData["Postmark"] = postmarkList;
            ViewData["Stamp"] = stamp;
            ViewData["Text"] = text;
            ViewData["Date"] = dateList;
            ViewData["Year"] = yearList;
            ViewData["Number"] = numberList;
            ViewData["Geography"] = geographyList;
            ViewData["Name"] = nameList;
            ViewData["Occasion"] = occasionList;
            ViewData["Street"] = streetList;

            ViewData["userId"] = _userManager.GetUserId(User);

            return View();
        }

        public ActionResult DetectTextInImages()
        {
            string dir = Path.Combine(_hostEnvironment.WebRootPath, "DetectedTexts_GoogleVision");
            if (!Directory.Exists(dir))
            {
                _ = Directory.CreateDirectory(dir);
            }

            DateTime timer1 = new();
            DateTime timer2 = new();
            List<TimeSpan> timeDifference = [];

            WordCategorizationModel wordCategorizationModel = new()
            {
                Cities =
                [
                    .. (from c in _dbIdentityContext.Oeconym
                        select c.OeconymName),
                ],
                Publishers =
                [
                    .. (from p in _dbIdentityContext.Manufactory
                        select p.ManufactoryName),
                ],
                AuthorArtists =
                [
                    .. (from aa in _dbIdentityContext.Person.Include(x => x.ProfessionICollection)
                        where aa.ProfessionICollection.Any(x => x.Name.Equals(1) || x.Name.Equals(2))
                        select aa.Name),
                ]
            };
            //var eraSelect = (from e in _dbIdentityContext.Era
            //                 select e.EraLong).ToList();

            foreach (string imageFileName in Directory.GetFiles("wwwroot/images/NormalSizeTestSchwierig"))
            {
                if (imageFileName != null)
                {
                    timer1 = DateTime.Now;
                    string txt = string.Empty;

                    //google API
                    string credPath = Path.Combine(_hostEnvironment.ContentRootPath, "google_application_default_credentials.json");
                    System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credPath);
                    ImageAnnotatorClient client = ImageAnnotatorClient.Create();
                    Google.Cloud.Vision.V1.Image newImage = Google.Cloud.Vision.V1.Image.FromFile(imageFileName);


                    try
                    {
                        IReadOnlyList<EntityAnnotation> textAnnotations = client.DetectText(newImage);
                        if (!textAnnotations.IsNullOrEmpty())
                        {
                            for (int tAIndex = 1; tAIndex < textAnnotations.Count; tAIndex++)
                            {
                                Google.Protobuf.Collections.RepeatedField<Vertex> vertices = textAnnotations[tAIndex].BoundingPoly.Vertices;
                                List<int> xPositions = vertices.Select(x => x.X).ToList();
                                List<int> yPositions = vertices.Select(x => x.Y).ToList();
                                wordCategorizationModel.WordsWithXYPositions.Add((textAnnotations[tAIndex].Description, xPositions, yPositions, wordCategorizationModel.Frontside));
                            }
                            if (wordCategorizationModel.Frontside)
                            {
                                WordCategorization.CheckWordPosComparedToCenterOfImg(wordCategorizationModel);
                            }
                        }
                    }
                    catch (AnnotateImageException e)
                    {
                        AnnotateImageResponse response = e.Response;
                        txt += "AnnotationError: " + response.Error;
                        wordCategorizationModel.CountErrors++;
                    }

                    if (wordCategorizationModel.WordsWithXYPositions.Count > 0)
                    {
                        if (!wordCategorizationModel.Frontside)
                        {
                            WordCategorization.CheckWordSize(wordCategorizationModel);
                            PicturePreprocess.CheckBlocks(wordCategorizationModel);
                            WordCategorization.CategorizeWord(wordCategorizationModel, _hostEnvironment);
                        }

                        string imageName = Path.GetFileNameWithoutExtension(imageFileName);
                        string file = Path.Combine(dir, imageName + ".txt");
                        StreamWriter stream = new(file, true);
                        if (wordCategorizationModel.Frontside)
                        {
                            stream.WriteLine("Vorderseite");
                        }
                        else
                        {
                            stream.WriteLine("Rückseite");
                        }

                        if (!string.IsNullOrEmpty(txt))
                        {
                            stream.WriteLine("---------------------------------------------------------");
                            stream.WriteLine("Fehler");
                            stream.WriteLine("---------------------------------------------------------");
                            stream.WriteLine(txt);
                        }
                        stream.WriteLine("---------------------------------------------------------");
                        stream.WriteLine("Blocks");
                        stream.WriteLine("---------------------------------------------------------");
                        foreach ((string content, string category, double prop, bool Frontside) block in wordCategorizationModel.Blocks)
                        {
                            stream.WriteLine(block);
                        }
                        stream.WriteLine("---------------------------------------------------------");
                        stream.WriteLine("Zugeordnet");
                        stream.WriteLine("---------------------------------------------------------");
                        foreach ((string Word, int Block, int Position, List<(double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) wbc in wordCategorizationModel.WordBlockCategorization)
                        {
                            stream.WriteLine(wbc);
                            foreach ((double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere) category in wbc.Category)
                            {
                                stream.WriteLine("  - " + category);
                            }
                        }

                        PicturePreprocess.CreateBlocks(wordCategorizationModel);
                        stream.WriteLine("---------------------------------------------------------");
                        stream.WriteLine("Blocks kategorisiert");
                        stream.WriteLine("---------------------------------------------------------");
                        foreach ((string content, string category, double prop, bool Frontside) block in wordCategorizationModel.Blocks)
                        {
                            stream.WriteLine(block);
                        }
                        stream.Close();
                    }

                    if (!wordCategorizationModel.Frontside)
                    {
                        wordCategorizationModel.MissSpelleds = [];
                        wordCategorizationModel.FoundAAs = [];
                        wordCategorizationModel.NewAAs = [];
                        wordCategorizationModel.RightSpelleds = [];
                        wordCategorizationModel.WordsWithXYPositions = [];
                        wordCategorizationModel.Buildings = [];
                        wordCategorizationModel.BuildingsWithXYPositionTupleList = [];
                        wordCategorizationModel.Frontside = true;
                        wordCategorizationModel.PostcardCategories = [];
                        wordCategorizationModel.WordBlockCategorization = [];
                        wordCategorizationModel.Blocks = [];
                    }
                    timer2 = DateTime.Now;
                    timeDifference.Add(timer2 - timer1);
                }
            }

            //var similaritiesInCities = SpellCheck.WordSimilarty(citySimilarities, 1);
            //var similaritiesInAAs = SpellCheck.WordSimilarty(authorArtistSimilarities, 1);

            StreamWriter stream2 = new(Path.Combine(dir, "Result_DetectTextInImages.txt"), true);
            stream2.WriteLine("Dauer: " + timeDifference.Average(timeSpan => timeSpan.TotalSeconds) + " || Maximal: " + timeDifference.Max()
                + " || Minimal: " + timeDifference.Min());
            stream2.WriteLine("Anzahl Objekte: " + wordCategorizationModel.Countobjects);
            stream2.WriteLine("Anzahl Fehler: " + wordCategorizationModel.CountErrors);
            stream2.Close();

            return RedirectToAction("AdministerCollectionPostcard", "PostcardDatabaseController");
        }

        public async Task<IActionResult> DownloadAnalysisResultYaml(PostcardAnalyzeResultParameters resultParameters, string userId, string pathFrontside, string pathBackside)
        {
            string downloadFolder = Path.Combine(_hostEnvironment.WebRootPath, "Download_" + userId);
            string zipFile = Path.Combine(_hostEnvironment.WebRootPath, "Download_" + userId + ".zip");
            PostcardModel postcardModel = new();

            _ = Directory.CreateDirectory(downloadFolder);
            DbActionsPostcard dbActionsPostcard = new(_dbIdentityContext, _userManager, processCity, logger2);
            dbActionsPostcard.ProcessAnalysisResultParameters(resultParameters, postcardModel);

            string targetFilePath = Path.Combine(downloadFolder, Path.GetFileName(pathFrontside));
            System.IO.File.Copy(pathFrontside, targetFilePath, true);
            targetFilePath = Path.Combine(downloadFolder, Path.GetFileName(pathBackside));
            System.IO.File.Copy(pathBackside, targetFilePath, true);

            string yamlFile = Path.Combine(downloadFolder, "PostcardDatas.yaml");
            if (!System.IO.File.Exists(yamlFile))
            {
                using FileStream sw = System.IO.File.Create(yamlFile);
                byte[] yaml = [];
                Sammlerplattform.Models.Download.PostcardDownloadModel postcardDownloadModel = YamlProcessor.ComposeForDownload(postcardModel, _dbIdentityContext, processCity);
                byte[] spty = YamlProcessor.SerializePostcardToYaml(postcardDownloadModel);
                sw.Write(spty);
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

        [GeneratedRegex(@"[<>'/;`%+{}\[\]‘\\°_*,?!():=.-]")]
        private static partial Regex RegexSpecialCharacter();
        [GeneratedRegex(@"[A-z1-9(]")]
        private static partial Regex RegexBeginningOfWord();
        [GeneratedRegex(@"[-.)]")]
        private static partial Regex RegexConjunctionWord();
    }
}
