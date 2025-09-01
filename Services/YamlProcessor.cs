using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.Download;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
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
                    Immaterial = operationParameterModel.BrickPotential.Immaterial,
                    SerialNumber = operationParameterModel.BrickPotential.SerialNumber,
                    FilingLocation = operationParameterModel.BrickEntity.FilingLocation,
                    Charge = operationParameterModel.BrickEntity.Charge,
                    Price = operationParameterModel.BrickEntity.DeliveryPrice,
                    Fake = operationParameterModel.BrickEntity.Fake,
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
                    Condition = operationParameterModel.BrickEntity.Condition?.ConditionName
                },
                Manufactorys = [.. from mc in operationParameterModel.BrickEntityNManufactoryNCityList
                                    select new YAMLManufactory
                                    {
                                        Name = mc.Manufactory?.ManufactoryName ?? string.Empty,
                                        City = mc.City?.CityOeconymList.FirstOrDefault(x => x.CurrentName)?.Oeconym.OeconymName ?? string.Empty,
                                        CityByname = mc.City?.Geography?.GeographyName ?? string.Empty,
                                        ProductionFacility = mc.Manufactory?.ProductionFacility?.ProductionFacilityName!
                                    }],
                People = [.. from person in operationParameterModel.BrickEntityNPersonList
                           select new YAMLPerson
                           {
                               Name = person.Person?.Name,
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

        
    }
}
