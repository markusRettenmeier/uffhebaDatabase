using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionAreaDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionAreaProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CollectionAreaDatabaseController(IProcessCollectionArea processCollection) : Controller
    {
        [HandleStatus]
        public ActionResult Index(CollectionAreaSearchParameterModel collectionSearchParameterModel)
        {
            List<CollectionArea> collectionList = processCollection
                .GetListWithPredicate(collectionSearchParameterModel);

            return View(collectionList);
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CollectionAreaCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }

            (int statusCode, string statusMessage, int id) = processCollection.Insert(createDTO);
            if (id == 0)
                return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
            else if (statusCode == 409)
                return RedirectToAction(nameof(Edit), new { statusMessage, statusCode, id });

            return RedirectToAction("Create", "ConceptualRelationshipDatabase", new { statusMessage, statusCode, collectionAreaID = id });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            CollectionArea? existingCollection = processCollection.
                GetListWithPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = [id] })
                .FirstOrDefault();
            if (existingCollection == null)
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound" });

            CollectionAreaEditDTO editDTO = new()
            {
                Id = existingCollection.CollectionAreaID,
                Name = existingCollection.CollectionAreaName,
                WikipediaUrl = existingCollection.WikipediaUrl
            };
            return View(editDTO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CollectionAreaEditDTO editDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(editDTO);
            }

            (int statusCode, string statusMessage, int id) = processCollection.Update(editDTO);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        public ActionResult Delete(int id)
        {
            CollectionArea? existingCollection = processCollection.
                GetListWithPredicate(new CollectionAreaSearchParameterModel { CollectionAreaID = [id] })
                .FirstOrDefault();
            return existingCollection == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CollectionArea_NotFound" })
                : View(existingCollection);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int collectionAreaID)
        {
            if (collectionAreaID <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });

            (int statusCode, string statusMessage) = processCollection.Delete(collectionAreaID);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}