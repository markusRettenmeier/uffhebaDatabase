using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Controllers;
using Sammlerplattform.Data;
using Sammlerplattform.Models;
using Sammlerplattform.Models.Download;
using YamlDotNet.Serialization;

namespace Sammlerplattform.Services
{
    public class YamlProcessor
    {
        public static byte[] SerializePostcardToYaml(PostcardDownloadModel model)
        {
            var serializer = new SerializerBuilder()
                .Build();
            var yaml = "# ----------Start----------\r\n" + serializer.Serialize(model);
            yaml += "# -----------End-----------";

            var buffer = new MemoryStream();
            using (var writer = new StreamWriter(buffer))
            {
                serializer.Serialize(writer, yaml);
            }

            var bytes = buffer.ToArray();

            return bytes;
        }

        public static PostcardDownloadModel ComposeForDownload(PostcardModel selectPostcard, DbIdentityContext dbIdentityContext, IProcessCity processCity)
        {
            CityParameterModel cityParameterModel = new();

            PostcardDownloadModel? postcardDownloadModel = new()
            {
                Scans = selectPostcard.PostcardScanList,
                Postcard = new Postcard
                {
                    CitiesOnPostcard = [.. (from c in dbIdentityContext.City.Include(x => x.PostalcodeICollection)
                                        join l in dbIdentityContext.Geography
                                        on c.Geography_ID equals l.Geography_ID into outerLeftGeography
                                        from subl in outerLeftGeography.DefaultIfEmpty()
                                        where c.PostcardPotentialList.Any(x => x.Product_ID.Equals(selectPostcard.PostcardPotential.Product_ID))
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
                    ProductionYear = selectPostcard.PostcardPotential.ProductionYear,
                    Immaterial = selectPostcard.PostcardPotential.Immaterial,
                    SerialNumber = selectPostcard.PostcardPotential.SerialNumber,
                    ISBN = selectPostcard.PostcardPotential.ISBN,
                    ProductionSize = selectPostcard.PostcardPotential.ProductionSize,
                    OfficialBusiness = selectPostcard.PostcardPotential.OfficialBusiness,
                    CorrugatedEdge = selectPostcard.PostcardPotential.CorrugatedEdge,
                    Fieldpost = selectPostcard.PostcardPotential.Fieldpost,
                    Formats = selectPostcard.PostcardPotential.Formats,
                    CardType = selectPostcard.PostcardPotential.CardType,
                    CardSeries = selectPostcard.PostcardPotential.CardSeries,
                    Leporello = selectPostcard.PostcardPotential.Leporello,
                    Propaganda = selectPostcard.PostcardPotential.Propaganda,
                    Ornament = selectPostcard.PostcardPotential.Ornament,
                    FilingLocation = selectPostcard.PostcardEntity.FilingLocation,
                    Charge = selectPostcard.PostcardEntity.Charge,
                    Price = selectPostcard.PostcardEntity.Price,
                    Fake = selectPostcard.PostcardEntity.Fake,
                    Material = selectPostcard.PostcardEntity.Material,
                    ColorRALWritingFrontside = ColorConverter.ArgbToHex(selectPostcard.PostcardEntity.ColorRALWritingFrontside),
                    ColorRALPrintingBackside = ColorConverter.ArgbToHex(selectPostcard.PostcardEntity.ColorRALPrintingBackside),
                    ConditionOfCard = selectPostcard.PostcardEntity.ConditionOfCard,
                    DateInText = selectPostcard.PostcardEntity.DateInText,
                    Text = selectPostcard.PostcardEntity.Text
                },
                Artist = new Sammlerplattform.Models.Download.Artist
                {
                    Name = selectPostcard.AuthorArtist?.AAName,
                    Description = selectPostcard.AuthorArtist?.ArtistDescription,
                    Prizes = selectPostcard.AuthorArtist?.Prizes,
                    Profession = selectPostcard.AuthorArtist?.Profession,
                    Signature = selectPostcard.AuthorArtist?.AASignature
                },
                Image = new Sammlerplattform.Models.Download.Image
                {
                    Height = selectPostcard.PostcardImprint?.Height,
                    Width = selectPostcard.PostcardImprint?.Width,
                    ColorProcessing = selectPostcard.PostcardImprint?.ColorProcessing,
                    ImageColor = ColorConverter.ArgbToHex(selectPostcard.PostcardImprint?.ColorImage_ID),
                    ImageYear = selectPostcard.PostcardImprint?.ImageYear,
                    EraLong = selectPostcard.Eras?.EraLong,
                    EraShort = selectPostcard.Eras?.EraShort,
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
                    Street = selectPostcard.PersonReceiver?.Street,
                    Streetnumber = selectPostcard.PersonReceiver?.HouseNumber,
                    City = (from c in processCity.GetCitiesWithPredicates(cityParameterModel)
                            where c.Person != null && selectPostcard.PersonReceiver != null && c.Person.Person_ID == selectPostcard.PersonReceiver.Person_ID
                            select new Sammlerplattform.Models.Download.City
                            {
                                Oeconym = c.CityNOeconymICollection.Select(x => x.Oeconym.OeconymName).ToList(),
                                Postalcode = c.PostalcodeICollection.Select(x => x.PostalcodeNumber).ToList(),
                                Byname = c.Byname,
                                Geography = c.Geography
                            }).FirstOrDefault()
                },
                Manufacturers = [.. (from p in dbIdentityContext.Manufacturer
                                 join pemc in dbIdentityContext.PostcardEntityNManufacturerNCity
                    on p.Manufacturer_ID equals pemc.Publisher_ID
                                 join c in dbIdentityContext.City.Include(x => x.CityNOeconymICollection).ThenInclude(x => x.Oeconym)
                                 on pemc.City_ID equals c.City_ID into leftOuterCity
                                 from subc in leftOuterCity.DefaultIfEmpty()
                                 where pemc.PostcardEntity_ID == selectPostcard.PostcardEntity.PostcardEntity_ID
                                 select new Sammlerplattform.Models.Download.Manufacturer
                                         {
                                              Name =  p.ManufacturerName,
                                     City = subc.CityNOeconymICollection.Where(x => x.CurrentName == true).Select(x => x.Oeconym.OeconymName).First(),
                                      CityByname = (from c in dbIdentityContext.City.Include(m => m.ManufacturerList)
                                              join l in dbIdentityContext.Geography
                                              on c.Geography_ID equals l.Geography_ID into leftOuterGeography
                                              from subl in leftOuterGeography.DefaultIfEmpty()
                                              where c.ManufacturerList.Any(c => c.Manufacturer_ID.Equals(p.Manufacturer_ID))
                                              select subl.GeographyName).FirstOrDefault()
                                          })]
            };
            return postcardDownloadModel;
        }
    }
}
