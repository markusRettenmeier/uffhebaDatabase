using Sammlerplattform.Models.CollectionItemDatabase;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using System.IO.Compression;
using YamlDotNet.Serialization;

namespace Sammlerplattform.Services
{
    public class YamlProcessor
    {
        public static async Task<MemoryStream> CreateZipFile(List<CollectionItemDisplayDTO> modelList, UsingIdentityUser user, IWebHostEnvironment hostEnvironment)
        {
            string sourceDir = Path.Combine(hostEnvironment.WebRootPath, Path.Combine("images", "Original"));
            string downloadFolder = Path.Combine(hostEnvironment.WebRootPath, "Download_" + user.DisplayName);
            _ = Directory.CreateDirectory(downloadFolder);
            string zipFile = Path.Combine(hostEnvironment.WebRootPath, "Download_" + user.DisplayName + ".zip");

            foreach (CollectionItemDisplayDTO operationParameterModel in modelList)
            {
                // Sicherstellen, dass CollectionItemEntity nicht null ist
                if (operationParameterModel.CollectionItemEntity == null)
                {
                    System.Diagnostics.Debug.WriteLine("Warnung: CollectionItemEntity ist null");
                    continue;
                }

                string itemId = operationParameterModel.CollectionItemEntity.CollectionItemEntityID.ToString();
                string yamlFolder = Path.Combine(downloadFolder, itemId);
                _ = Directory.CreateDirectory(yamlFolder);

                string yamlFile = Path.Combine(yamlFolder, "PostcardDatas.yaml");
                using FileStream sw = File.Create(yamlFile);
                byte[] yamlBytes = CreateYAMLFile(operationParameterModel);
                await sw.WriteAsync(yamlBytes);

                // Sicherstellen, dass CollectionItemPictureList nicht null ist
                if (operationParameterModel.CollectionItemPictureList != null)
                {
                    foreach (CollectionItemPicture scan in operationParameterModel.CollectionItemPictureList)
                    {
                        string sourceFilePath = Path.Combine(sourceDir, scan.CollectionItemPictureID.ToString() + ".png");
                        string targetFilePath = Path.Combine(yamlFolder, scan.CollectionItemPictureID.ToString() + ".png");
                        if (File.Exists(sourceFilePath))
                        {
                            File.Copy(sourceFilePath, targetFilePath, true);
                        }
                    }
                }
            }

            // Überprüfen ob Ordner Inhalte hat bevor ZIP erstellt wird
            if (!Directory.EnumerateFileSystemEntries(downloadFolder).Any())
            {
                return new MemoryStream();
            }

            ZipFile.CreateFromDirectory(downloadFolder, zipFile);
            MemoryStream memory = new();
            using (FileStream stream = new(zipFile, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // Cleanup
            try
            {
                Directory.Delete(downloadFolder, true);
                File.Delete(zipFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cleanup error: {ex.Message}");
            }

            return memory;
        }

        public static byte[] CreateYAMLFile(CollectionItemDisplayDTO operationParameterModel)
        {
            var exportDto = new YamlExportDto
            {
                Scans = [.. operationParameterModel.CollectionItemPictureList.Select(p => new YamlScanDto
                {
                    CollectionItemPictureID = p.CollectionItemPictureID,
                    Perspective = p.Perspective.ToString(),
                    FileName = $"{p.CollectionItemPictureID}.png"
                })],

                CollectionItem = new YamlCollectionItemDto
                {
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
                    Condition = operationParameterModel.CollectionItemEntity.StatePreservation?.StatePreservationName,
                    //Parties = operationParameterModel.CollectionItemEntity.CollectionItemNParticipantList?
                    //    .ToDictionary(p => p.Relationship, p => p.Participant.ParticipantName),
                    //Places = operationParameterModel.CollectionItemEntity.CollectionItemNPlaceList?
                    //    .Select(pl => pl.Place.PlaceNToponymyList?.FirstOrDefault()?.Toponymy?.ToponymyName)
                    //    .Where(name => !string.IsNullOrEmpty(name))
                    //    .ToList(),
                    Era = operationParameterModel.CollectionItemEntity.Era?.EraName
                }
            };

            return SerializeCollectionItemToYaml(exportDto);
        }
        public static byte[] SerializeCollectionItemToYaml(YamlExportDto exportDto)
        {
            ISerializer serializer = new SerializerBuilder()
                .Build();
            string yaml = "# ----------Start----------\r\n" + serializer.Serialize(exportDto);
            yaml += "# -----------End-----------";

            MemoryStream buffer = new();
            using (StreamWriter writer = new(buffer))
            {
                serializer.Serialize(writer, yaml);
            }

            byte[] bytes = buffer.ToArray();

            return bytes;
        }

        // Spezielle DTOs für YAML-Export
        public class YamlScanDto
        {
            public int CollectionItemPictureID { get; set; }
            public string? Perspective { get; set; }
            public string? FileName { get; set; }
        }

        public class YamlCollectionItemDto
        {
            public string? FilingLocation { get; set; }
            public decimal? Price { get; set; }
            public bool Fake { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
            public decimal? Length { get; set; }
            public int? ExactYear { get; set; }
            public int? StartYear { get; set; }
            public int? EndYear { get; set; }
            public bool IsApproximate { get; set; }
            public string? Comment { get; set; }
            public string? Condition { get; set; }
            public List<string>? Colors { get; set; }
            public List<string>? Materials { get; set; }
            public Dictionary<string, string>? Parties { get; set; }
            public List<string?>? Places { get; set; }
            public string? Concept { get; set; }
            public string? Era { get; set; }
        }

        public class YamlExportDto
        {
            public List<YamlScanDto> Scans { get; set; } = [];
            public YamlCollectionItemDto CollectionItem { get; set; } = new();
        }
    }
}