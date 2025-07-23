using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using Sammlerplattform.Services.Processes;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class BrickDatabaseController(IProcessBrick processBrick, UserManager<UsingIdentityUser> userManager, IWebHostEnvironment hostEnvironment) : Controller
    {
        public ActionResult AdministerCollectionBrick(string statusMessage, BrickSearchParameterModel model)
        {
            ViewData["StatusMessage"] = statusMessage;

            string userId = userManager.GetUserId(User) ?? throw new NullReferenceException();
            model.UsingIdentityUsersID.Add(userId);
            return View(processBrick.GetWithPredicates(model));
        }

        public ActionResult CreateBrick(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View();
        }
        public async Task<IActionResult> CreateBrickSubmit(BrickOperationParameterModel brickOperationParameter)
        {
            brickOperationParameter.BrickEntity.UsingIdentityUsersID = userManager.GetUserId(User) ?? throw new NullReferenceException();
            Task<UsingIdentityUser?> user = userManager.GetUserAsync(User);
            brickOperationParameter.BrickEntity.UsingIdentityUser = await user;
            (BrickEntity brickEntity, int _, string statusMessage) = processBrick.Create(brickOperationParameter);

            return RedirectToAction(nameof(EditBrick), new { statusMessage, entityId = brickEntity.BrickEntityID });
        }

        public ActionResult EditBrick(string statusMessage, int entityId)
        {
            ViewData["StatusMessage"] = statusMessage;

            BrickSearchParameterModel brickSearch = new();
            brickSearch.BrickEntityID.Add(entityId);
            BrickOperationParameterModel model = processBrick.GetWithPredicates(brickSearch).First();
            return View(model);
        }
        public async Task<ActionResult> EditBrickSubmit(BrickOperationParameterModel brickOperationParameter)
        {
            Task<UsingIdentityUser?> user = userManager.GetUserAsync(User);
            brickOperationParameter.BrickEntity.UsingIdentityUser = await user;
            (BrickEntity brickEntity, int _, string statusMessage) = processBrick.Edit(brickOperationParameter);

            return RedirectToAction(nameof(EditBrick), new { statusMessage, entityId = brickEntity.BrickEntityID });
        }

        public ActionResult DeleteBrick(string statusMessage, int entityId)
        {
            ViewData["StatusMessage"] = statusMessage;

            BrickSearchParameterModel brickSearch = new();
            brickSearch.BrickEntityID.Add(entityId);
            return View(processBrick.GetWithPredicates(brickSearch).First());
        }
        public IActionResult DeleteBrickSubmit(BrickOperationParameterModel model)
        {
            (BrickEntity brickEntity, int _, string statusMessage) = processBrick.Delete(model);

            return RedirectToAction(nameof(AdministerCollectionBrick), new { statusMessage });
        }

        public async Task<ActionResult> DownloadBrickSubmit(int? entityId)
        {
            Task<UsingIdentityUser?> userTask = userManager.GetUserAsync(User);
            UsingIdentityUser? user = await userTask;
            if(user == null)
            {
                return RedirectToAction(nameof(AdministerCollectionBrick), new { statusMessage = "User wurde nicht gefunden." });
            }

            BrickSearchParameterModel brickSearch = new();
            if (entityId > 0)
            {
                brickSearch.BrickEntityID.Add((int)entityId);
            }
            brickSearch.UsingIdentityUsersID.Add(user.Id);
            List<BrickOperationParameterModel> modelList = [.. processBrick.GetWithPredicates(brickSearch)];
            MemoryStream memory = await YamlProcessor.CreateZipFile(modelList, user, hostEnvironment);

            return File(memory, "application/zip", "Download_" + user.UserName + ".zip");
        }

        //private async Task<MemoryStream> Test(int? entityId, UsingIdentityUser? user)
        //{
        //    string sourceDir = Path.Combine(_hostEnvironment.WebRootPath, Path.Combine("images", "Original"));
        //    string downloadFolder = Path.Combine(_hostEnvironment.WebRootPath, "Download_" + user.UserName);
        //    _ = Directory.CreateDirectory(downloadFolder);
        //    string zipFile = Path.Combine(_hostEnvironment.WebRootPath, "Download_" + user.UserName + ".zip");

        //    BrickSearchParameterModel brickSearch = new();
        //    if (entityId > 0)
        //    {
        //        brickSearch.SearchBrickEntityID.Add((int)entityId);
        //    }
        //    brickSearch.SearchUsingIdentityUsersID.Add(user.Id);
        //    List<BrickOperationParameterModel> modelList = [.. processBrick.GetWithPredicates(brickSearch)];

        //    foreach (BrickOperationParameterModel operationParameterModel in modelList)
        //    {
        //        string yamlFolder = Path.Combine(downloadFolder, operationParameterModel.BrickEntity.BrickEntityID.ToString());
        //        _ = Directory.CreateDirectory(yamlFolder);

        //        string yamlFile = Path.Combine(yamlFolder, "PostcardDatas.yaml");
        //        using FileStream sw = System.IO.File.Create(yamlFile);
        //        byte[] yamlBytes = YamlProcessor.CreateYAMLFile(operationParameterModel);
        //        sw.Write(yamlBytes);

        //        foreach (ProductPicture scan in operationParameterModel.ProductPictureList)
        //        {
        //            string sourceFilePath = Path.Combine(sourceDir, scan.ProductPictureID.ToString() + ".png");
        //            string targetFilePath = Path.Combine(yamlFolder, scan.ProductPictureID.ToString() + ".png");
        //            System.IO.File.Copy(sourceFilePath, targetFilePath, true);
        //        }
        //    }

        //    ZipFile.CreateFromDirectory(downloadFolder, zipFile);
        //    MemoryStream memory = new();
        //    using (FileStream stream = new(zipFile, FileMode.Open))
        //    {
        //        await stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;
        //    Directory.Delete(downloadFolder, true);
        //    System.IO.File.Delete(zipFile);
        //    return memory;
        //}
        //public ActionResult DownloadAllBricksSubmit()
        //{
        //    string userId = userManager.GetUserId(User) ?? throw new NullReferenceException();



        //    return File(memory, "application/zip", Path.GetFileName(zipFile));
        //}
    }
}
