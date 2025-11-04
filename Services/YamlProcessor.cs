using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.Download;
using Sammlerplattform.Models.UserSettings;
using System.IO.Compression;
using YamlDotNet.Serialization;

namespace Sammlerplattform.Services
{
    public class YamlProcessor
    {
        public static async Task<MemoryStream> CreateZipFile(List<CollectionItemOperationParameterModel> modelList, UsingIdentityUser user, IWebHostEnvironment hostEnvironment)
        {
            string sourceDir = Path.Combine(hostEnvironment.WebRootPath, Path.Combine("images", "Original"));
            string downloadFolder = Path.Combine(hostEnvironment.WebRootPath, "Download_" + user.UserName);
            _ = Directory.CreateDirectory(downloadFolder);
            string zipFile = Path.Combine(hostEnvironment.WebRootPath, "Download_" + user.UserName + ".zip");

            foreach (CollectionItemOperationParameterModel operationParameterModel in modelList)
            {
                string yamlFolder = Path.Combine(downloadFolder, operationParameterModel.CollectionItemEntity.CollectionItemEntityID.ToString());
                _ = Directory.CreateDirectory(yamlFolder);

                string yamlFile = Path.Combine(yamlFolder, "PostcardDatas.yaml");
                using FileStream sw = File.Create(yamlFile);
                byte[] yamlBytes = CreateYAMLFile(operationParameterModel);
                sw.Write(yamlBytes);

                foreach (CollectionItemPicture scan in operationParameterModel.CollectionItemPictureList)
                {
                    string sourceFilePath = Path.Combine(sourceDir, scan.CollectionItemPictureID.ToString() + ".png");
                    string targetFilePath = Path.Combine(yamlFolder, scan.CollectionItemPictureID.ToString() + ".png");
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

        public static byte[] CreateYAMLFile(CollectionItemOperationParameterModel operationParameterModel)
        {
            YAMLCollectionItemDownloadModel collectionItemDownloadModel = new()
            {
                Scans = operationParameterModel.CollectionItemPictureList,
                Product = new YAMLCollectionItem
                {
                    //SerialNumber = operationParameterModel.CollectionItemEntity.SerialNumber,
                    FilingLocation = operationParameterModel.CollectionItemEntity.FilingLocation,
                    Price = operationParameterModel.CollectionItemEntity.DeliveryPrice,
                    Fake = operationParameterModel.CollectionItemEntity.Fake,
                    Width = operationParameterModel.CollectionItemEntity.Width,
                    Height = operationParameterModel.CollectionItemEntity.Height,
                    Length = operationParameterModel.CollectionItemEntity.Length,
                    ExactYear = operationParameterModel.CollectionItemEntity.ExactYear,
                    StartYear = operationParameterModel.CollectionItemEntity.StartYear,
                    EndYear = operationParameterModel.CollectionItemEntity.EndYear,
                    IsApproximate = operationParameterModel.CollectionItemEntity.IsApproximate,
                    Comment = operationParameterModel.CollectionItemEntity.Comment,
                    TransferFromOwner = operationParameterModel.CollectionItemEntity.TransferFromOwner.ToString(),
                    ProductionSize = operationParameterModel.CollectionItemEntity.ProductionSize,
                    Condition = operationParameterModel.CollectionItemEntity.State?.StateName
                }
            };

            byte[] yamlBytes = SerializeProductToYaml(collectionItemDownloadModel);
            using MemoryStream memoryStream = new();
            return yamlBytes;
        }

        public static byte[] SerializeProductToYaml(YAMLCollectionItemDownloadModel model)
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
