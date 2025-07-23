using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.Download;
using Sammlerplattform.Models.PostcardDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services.Processes.CityProcesses;
using System.IO.Compression;
using YamlDotNet.Serialization;

namespace Sammlerplattform.Services
{
    public class YamlProcessor
    {
        public static async Task<MemoryStream> CreateZipFile(List<BrickOperationParameterModel> modelList, UsingIdentityUser user, IWebHostEnvironment hostEnvironment)
        {
            string sourceDir = Path.Combine(hostEnvironment.WebRootPath, Path.Combine("images", "Original"));
            string downloadFolder = Path.Combine(hostEnvironment.WebRootPath, "Download_" + user.UserName);
            _ = Directory.CreateDirectory(downloadFolder);
            string zipFile = Path.Combine(hostEnvironment.WebRootPath, "Download_" + user.UserName + ".zip");

            foreach (BrickOperationParameterModel operationParameterModel in modelList)
            {
                string yamlFolder = Path.Combine(downloadFolder, operationParameterModel.BrickEntity.BrickEntityID.ToString());
                _ = Directory.CreateDirectory(yamlFolder);

                string yamlFile = Path.Combine(yamlFolder, "PostcardDatas.yaml");
                using FileStream sw = File.Create(yamlFile);
                byte[] yamlBytes = CreateYAMLFile(operationParameterModel);
                sw.Write(yamlBytes);

                foreach (ProductPicture scan in operationParameterModel.ProductPictureList)
                {
                    string sourceFilePath = Path.Combine(sourceDir, scan.ProductPictureID.ToString() + ".png");
                    string targetFilePath = Path.Combine(yamlFolder, scan.ProductPictureID.ToString() + ".png");
                    File.Copy(sourceFilePath, targetFilePath, true);
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
            File.Delete(zipFile);
            return memory;
        }

        public static byte[] CreateYAMLFile(BrickOperationParameterModel operationParameterModel)
        {
            YAMLBrickDownloadModel brickDownloadModel = new()
                {
                    Scans = operationParameterModel.ProductPictureList,
                Brick = new YAMLBrick
                {
                    Brickname = string.Join(", ", operationParameterModel.BrickPotential.BricknameSynonymList.Select(x => x.Name)),
                    Usage = operationParameterModel.BrickPotential.UsageEnumDescription,
                    Relief = operationParameterModel.BrickEntity.ReliefEnum.ToString(),
                    Keyword = operationParameterModel.BrickEntity.KeywordEnumDescription,
                    Immaterial = operationParameterModel.BrickPotential.Immaterial,
                    SerialNumber = operationParameterModel.BrickPotential.SerialNumber,
                    FilingLocation = operationParameterModel.BrickEntity.FilingLocation,
                    Charge = operationParameterModel.BrickEntity.Charge,
                    Price = operationParameterModel.BrickEntity.Price,
                    Fake = operationParameterModel.BrickEntity.Fake,
                    MaterialEnum = operationParameterModel.BrickEntity.MaterialEnum.ToString(),
                    Width = operationParameterModel.BrickEntity.Width,
                    Height = operationParameterModel.BrickEntity.Height,
                    Length = operationParameterModel.BrickEntity.Length,
                    ExactYear = operationParameterModel.BrickEntity.ExactYear,
                    StartYear = operationParameterModel.BrickEntity.StartYear,
                    EndYear = operationParameterModel.BrickEntity.EndYear,
                    IsApproximate = operationParameterModel.BrickEntity.IsApproximate,
                    Comment = operationParameterModel.BrickEntity.Comment,
                    TransferFromOwner = operationParameterModel.BrickEntity.TransferFromOwner.ToString(),
                    ProductionSize = operationParameterModel.BrickEntity.ProductionSize,
                    Condition = operationParameterModel.BrickEntity.ConditionEnum.ToString()
                },
                Manufactorys = [.. from mc in operationParameterModel.BrickEntityNManufactoryNCityList
                                    select new YAMLManufactory
                                    {
                                        Name = mc.Manufactory?.ManufactoryName ?? string.Empty,
                                        City = mc.City?.CityNOeconymList.FirstOrDefault(x => x.CurrentName)?.Oeconym.OeconymName ?? string.Empty,
                                        CityByname = mc.City?.Geography?.GeographyName ?? string.Empty,
                                        ProductionFacility = mc.Manufactory?.ProductionFacility?.ProductionFacilityName!
                                    }],
                People = [.. from person in operationParameterModel.BrickEntityNPersonList
                           select new YAMLPerson
                           {
                               Name = person.Person?.Name,
                               City = person.Person?.City?.CityNOeconymList.FirstOrDefault(x => x.CurrentName)!.Oeconym.OeconymName,
                               Pseudonym = person.Person?.Pseudonym,
                               Signature = person.Person?.Signature,
                               Description = person.Person?.PersonDescription,
                               Relation_To_Brick = person.Relationship
                           }]
            };

            byte[] yamlBytes = SerializeBrickToYaml(brickDownloadModel);
            using MemoryStream memoryStream = new();
            return yamlBytes;
        }

        public static byte[] SerializeBrickToYaml(YAMLBrickDownloadModel model)
        {
            ISerializer serializer = new SerializerBuilder()
                .Build();
            string yaml = "# ----------Start----------\r\n" + serializer.Serialize(model);
            yaml += "# -----------End-----------";

            MemoryStream buffer = new();
            using (StreamWriter writer = new(buffer))
            {
                serializer.Serialize(writer, yaml);
            }

            byte[] bytes = buffer.ToArray();

            return bytes;
        }

        public static byte[] SerializePostcardToYaml(PostcardDownloadModel model)
        {
            ISerializer serializer = new SerializerBuilder()
                .Build();
            string yaml = "# ----------Start----------\r\n" + serializer.Serialize(model);
            yaml += "# -----------End-----------";

            MemoryStream buffer = new();
            using (StreamWriter writer = new(buffer))
            {
                serializer.Serialize(writer, yaml);
            }

            byte[] bytes = buffer.ToArray();

            return bytes;
        }

        public static PostcardDownloadModel ComposeForDownload(PostcardModel selectPostcard, DbIdentityContext dbIdentityContext, IProcessCity processCity)
        {
            CityOperationParameterModel cityParameterModel = new();

            string? test = ((MaterialType)selectPostcard.PostcardEntity.MaterialInt).ToString();

            PostcardDownloadModel? postcardDownloadModel = new()
            {
                Scans = selectPostcard.ProductPictureList,
                Postcard = new YAMLPostcard
                {
                    CitiesOnPostcard = [.. from c in dbIdentityContext.City.Include(x => x.PostalcodeList)
                                        join l in dbIdentityContext.Geography
                                        on c.GeographyID equals l.Geography_ID into outerLeftGeography
                                        from subl in outerLeftGeography.DefaultIfEmpty()
                                        where c.PostcardPotentialList.Any(x => x.PostcardPotential_ID.Equals(selectPostcard.PostcardPotential.PostcardPotential_ID))
                                        select new Sammlerplattform.Models.Download.YAMLCity
                                        {
                                            Oeconym = dbIdentityContext.City.Where(x => x.CityID.Equals(c.CityID)).SelectMany(x => x.CityNOeconymList.Select(y =>
                                                       y.Oeconym.OeconymName
                                                   )).ToList(),
                                            Postalcode = dbIdentityContext.City.Where(x => x.CityID.Equals(c.CityID)).SelectMany(x => x.PostalcodeList.Select(y =>
                                                       y.PostalcodeNumber
                                                   )).ToList(),
                                            Byname = subl.GeographyName
                                        }],
                    Immaterial = selectPostcard.PostcardPotential.Immaterial,
                    SerialNumber = selectPostcard.PostcardPotential.SerialNumber,
                    ProductionSize = selectPostcard.PostcardEntity.ProductionSize,
                    Formats = selectPostcard.PostcardPotential.Formats,
                    CardType = selectPostcard.PostcardPotential.CardType,
                    CardSeries = selectPostcard.PostcardPotential.CardSeries,
                    FilingLocation = selectPostcard.PostcardEntity.FilingLocation,
                    Charge = selectPostcard.PostcardEntity.Charge,
                    Price = selectPostcard.PostcardEntity.Price,
                    Fake = selectPostcard.PostcardEntity.Fake,
                    Material = selectPostcard.PostcardEntity.MaterialEnum.ToString(),
                    ColorRALWritingFrontside = ColorConverter.ArgbToHex(selectPostcard.PostcardEntity.ColorRALWritingFrontside),
                    ColorRALPrintingBackside = ColorConverter.ArgbToHex(selectPostcard.PostcardEntity.ColorRALPrintingBackside),
                    ConditionOfCard = selectPostcard.PostcardEntity.ConditionEnum.ToString(),
                    Text = selectPostcard.PostcardEntity.Text
                },
                Artist = new Sammlerplattform.Models.Download.YAMLArtist
                {
                    Name = selectPostcard.AuthorArtist?.Name,
                    Description = selectPostcard.AuthorArtist?.PersonDescription,
                    Prizes = selectPostcard.AuthorArtist?.PrizeICollection.Select(x => x.Name).ToString(),
                    Profession = selectPostcard.AuthorArtist?.ProfessionICollection.Select(x => x.Name).ToString(),
                    Signature = selectPostcard.AuthorArtist?.Signature
                },
                Image = new Sammlerplattform.Models.Download.YAMLImage
                {
                    Height = selectPostcard.PostcardImprint?.Height,
                    Width = selectPostcard.PostcardImprint?.Width,
                    ColorProcessing = selectPostcard.PostcardImprint?.ColorProcessing,
                    ImageColor = ColorConverter.ArgbToHex(selectPostcard.PostcardImprint?.ColorImage_ID),
                    ImageYear = selectPostcard.PostcardImprint?.ImageYear,
                    EraLong = selectPostcard.Era?.EraName,
                    EraShort = selectPostcard.Era?.EraShort,
                    Passepartout = selectPostcard.PostcardImprint?.Passepartout,
                    FullScreen = selectPostcard.PostcardImprint?.FullScreen,
                    CirculationSize = selectPostcard.PostcardImprint?.CirculationSize,
                    Buildings = selectPostcard.PostcardImprint?.Buildings,
                    Technique = selectPostcard.Printing?.Technique,
                    Style = selectPostcard.Printing?.Style
                },
                Sender = selectPostcard.PersonSender?.Name,
                Receiver = new Sammlerplattform.Models.Download.YAMLReceiver
                {
                    Name = selectPostcard.PersonReceiver?.Name,
                    City = (from c in processCity.GetCityOPMWithPredicates(processCity.CityParametersOperationToSearch(cityParameterModel)).Select(x => x.City)
                            where c.PersonList != null && selectPostcard.PersonReceiver != null && c.PersonList.Any(x => x.PersonID == selectPostcard.PersonReceiver.PersonID)
                            select new Sammlerplattform.Models.Download.YAMLCity
                            {
                                Oeconym = [.. c.CityNOeconymList.Select(x => x.Oeconym.OeconymName)],
                                Postalcode = [.. c.PostalcodeList.Select(x => x.PostalcodeNumber)],
                                Byname = c.Byname,
                                Geography = c.Geography
                            }).FirstOrDefault()
                },
                Manufactorys = [.. from p in dbIdentityContext.Manufactory
                                 join pemc in dbIdentityContext.PostcardEntityNManufactoryNCity
                                on p.ManufactoryID equals pemc.Publisher_ID
                                 join c in dbIdentityContext.City.Include(x => x.CityNOeconymList).ThenInclude(x => x.Oeconym)
                                 on pemc.City_ID equals c.CityID into leftOuterCity
                                 from subc in leftOuterCity.DefaultIfEmpty()
                                 where pemc.PostcardEntity_ID == selectPostcard.PostcardEntity.PostcardEntity_ID
                                 select new Sammlerplattform.Models.Download.YAMLManufactory
                                         {
                                              Name =  p.ManufactoryName,
                                     City = subc.CityNOeconymList.Where(x => x.CurrentName == true).Select(x => x.Oeconym.OeconymName).First(),
                                      CityByname = (from c in dbIdentityContext.City.Include(m => m.ManufactoryList)
                                              join l in dbIdentityContext.Geography
                                              on c.GeographyID equals l.Geography_ID into leftOuterGeography
                                              from subl in leftOuterGeography.DefaultIfEmpty()
                                              where c.ManufactoryList.Any(c => c.ManufactoryID.Equals(p.ManufactoryID))
                                              select subl.GeographyName).FirstOrDefault()!
                                          }]
            };
            return postcardDownloadModel;
        }
    }
}
