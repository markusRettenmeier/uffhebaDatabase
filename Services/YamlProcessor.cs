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

            foreach (CollectionItemDisplayDTO collectionItemDisplaydto in modelList)
            {
                // Sicherstellen, dass CollectionItemEntity nicht null ist
                if (collectionItemDisplaydto == null)
                {
                    System.Diagnostics.Debug.WriteLine("Warnung: CollectionItemEntity ist null");
                    continue;
                }

                string itemId = collectionItemDisplaydto.CollectionItemEntityID.ToString();
                string yamlFolder = Path.Combine(downloadFolder, itemId);
                _ = Directory.CreateDirectory(yamlFolder);

                string yamlFile = Path.Combine(yamlFolder, "PostcardDatas.yaml");
                using FileStream sw = File.Create(yamlFile);
                byte[] yamlBytes = CreateYAMLFile(collectionItemDisplaydto);
                await sw.WriteAsync(yamlBytes);

                // Sicherstellen, dass CollectionItemPictureList nicht null ist
                if (collectionItemDisplaydto.CollectionItemPictureList != null)
                {
                    foreach (CollectionItemPicture scan in collectionItemDisplaydto.CollectionItemPictureList)
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

        public static byte[] CreateYAMLFile(CollectionItemDisplayDTO collectionItemDisplay)
        {
            var exportDto = new YamlExportDto
            {
                Scans = [.. collectionItemDisplay.CollectionItemPictureList.Select(p => new YamlScanDto
                {
                    CollectionItemPictureID = p.CollectionItemPictureID,
                    Perspective = p.Perspective.ToString(),
                    FileName = $"{p.CollectionItemPictureID}.png"
                })],

                CollectionItem = new YamlCollectionItemDto
                {
                    FilingLocation = collectionItemDisplay.FilingLocation,
                    Price = collectionItemDisplay.DeliveryPrice,
                    Fake = collectionItemDisplay.Fake,
                    Width = collectionItemDisplay.Width,
                    Height = collectionItemDisplay.Height,
                    Length = collectionItemDisplay.Length,
                    ExactYear = collectionItemDisplay.ExactYear,
                    StartYear = collectionItemDisplay.StartYear,
                    EndYear = collectionItemDisplay.EndYear,
                    IsApproximate = collectionItemDisplay.IsApproximate,
                    Comment = collectionItemDisplay.Comment,
                    Condition = collectionItemDisplay.StatePreservationName,
                    //Parties = collectionItemDisplaydto.CollectionItemEntity.CollectionItemNParticipantList?
                    //    .ToDictionary(p => p.Relationship, p => p.Participant.ParticipantName),
                    //Places = collectionItemDisplaydto.CollectionItemEntity.CollectionItemNPlaceList?
                    //    .Select(pl => pl.Place.ToponymyList?.FirstOrDefault()?.Toponymy?.ToponymyName)
                    //    .Where(name => !string.IsNullOrEmpty(name))
                    //    .ToList(),
                    Era = collectionItemDisplay.EraName
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