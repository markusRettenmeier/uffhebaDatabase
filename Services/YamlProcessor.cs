using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Controllers;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.Download;
using Sammlerplattform.Models.ProductDatabase;
using YamlDotNet.Serialization;

namespace Sammlerplattform.Services
{
    public class YamlProcessor
    {
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
                Postcard = new Postcard
                {
                    CitiesOnPostcard = [.. (from c in dbIdentityContext.City.Include(x => x.PostalcodeICollection)
                                        join l in dbIdentityContext.Geography
                                        on c.Geography_ID equals l.Geography_ID into outerLeftGeography
                                        from subl in outerLeftGeography.DefaultIfEmpty()
                                        where c.PostcardPotentialList.Any(x => x.PostcardPotential_ID.Equals(selectPostcard.PostcardPotential.PostcardPotential_ID))
                                        select new Sammlerplattform.Models.Download.City
                                        {
                                            Oeconym = dbIdentityContext.City.Where(x => x.City_ID.Equals(c.City_ID)).SelectMany(x => x.CityNOeconymICollection.Select(y =>
                                                       y.Oeconym.OeconymName
                                                   )).ToList(),
                                            Postalcode = dbIdentityContext.City.Where(x => x.City_ID.Equals(c.City_ID)).SelectMany(x => x.PostalcodeICollection.Select(y =>
                                                       y.PostalcodeNumber
                                                   )).ToList(),
                                            Byname = subl.GeographyName
                                        })],
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
                Artist = new Sammlerplattform.Models.Download.Artist
                {
                    Name = selectPostcard.AuthorArtist?.Name,
                    Description = selectPostcard.AuthorArtist?.PersonDescription,
                    Prizes = selectPostcard.AuthorArtist?.PrizeICollection.Select(x => x.Name).ToString(),
                    Profession = selectPostcard.AuthorArtist?.ProfessionICollection.Select(x => x.Name).ToString(),
                    Signature = selectPostcard.AuthorArtist?.PersonSignature
                },
                Image = new Sammlerplattform.Models.Download.Image
                {
                    Height = selectPostcard.PostcardImprint?.Height,
                    Width = selectPostcard.PostcardImprint?.Width,
                    ColorProcessing = selectPostcard.PostcardImprint?.ColorProcessing,
                    ImageColor = ColorConverter.ArgbToHex(selectPostcard.PostcardImprint?.ColorImage_ID),
                    ImageYear = selectPostcard.PostcardImprint?.ImageYear,
                    EraLong = selectPostcard.Era?.EraLong,
                    EraShort = selectPostcard.Era?.EraShort,
                    Passepartout = selectPostcard.PostcardImprint?.Passepartout,
                    FullScreen = selectPostcard.PostcardImprint?.FullScreen,
                    CirculationSize = selectPostcard.PostcardImprint?.CirculationSize,
                    Buildings = selectPostcard.PostcardImprint?.Buildings,
                    Technique = selectPostcard.Printing?.Technique,
                    Style = selectPostcard.Printing?.Style
                },
                Sender = selectPostcard.PersonSender?.Name,
                Receiver = new Sammlerplattform.Models.Download.Receiver
                {
                    Name = selectPostcard.PersonReceiver?.Name,
                    City = (from c in processCity.GetCityWithPredicates(processCity.CityParametersOperationToSearch(cityParameterModel))
                            where c.PersonICollection != null && selectPostcard.PersonReceiver != null && c.PersonICollection.Any(x => x.Person_ID == selectPostcard.PersonReceiver.Person_ID)
                            select new Sammlerplattform.Models.Download.City
                            {
                                Oeconym = c.CityNOeconymICollection.Select(x => x.Oeconym.OeconymName).ToList(),
                                Postalcode = c.PostalcodeICollection.Select(x => x.PostalcodeNumber).ToList(),
                                Byname = c.Byname,
                                Geography = c.Geography
                            }).FirstOrDefault()
                },
                Manufactorys = [.. (from p in dbIdentityContext.Manufactory
                                 join pemc in dbIdentityContext.PostcardEntityNManufactoryNCity
                                on p.Manufactory_ID equals pemc.Publisher_ID
                                 join c in dbIdentityContext.City.Include(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                                 on pemc.City_ID equals c.City_ID into leftOuterCity
                                 from subc in leftOuterCity.DefaultIfEmpty()
                                 where pemc.PostcardEntity_ID == selectPostcard.PostcardEntity.PostcardEntity_ID
                                 select new Sammlerplattform.Models.Download.Manufactory
                                         {
                                              Name =  p.ManufactoryName,
                                     City = subc.CityNOeconymICollection.Where(x => x.CurrentName == true).Select(x => x.Oeconym.OeconymName).First(),
                                      CityByname = (from c in dbIdentityContext.City.Include(m => m.ManufactoryList)
                                              join l in dbIdentityContext.Geography
                                              on c.Geography_ID equals l.Geography_ID into leftOuterGeography
                                              from subl in leftOuterGeography.DefaultIfEmpty()
                                              where c.ManufactoryList.Any(c => c.Manufactory_ID.Equals(p.Manufactory_ID))
                                              select subl.GeographyName).FirstOrDefault()
                                          })]
            };
            return postcardDownloadModel;
        }
    }
}
