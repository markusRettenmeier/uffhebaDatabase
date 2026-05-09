using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class PlaceDatabaseController(IProcessPlace processPlace
        , ITrackEventsCSV trackEvents) : Controller
    {
        [HandleStatus]
        public ActionResult Index(PlaceSearchParameterModel placeSearchParameter)
        {
            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter)];
            return View(placeList);
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PlaceCreateDTO model)
        {
            // 1 Model bereinigen
            model.ToponymyList = [.. model.ToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Name))];
            // 2 ModelState neu validieren, da sonst bei leeren Feldern der Fehler "The ToName field is required." angezeigt wird, obwohl das Feld ja nicht zwingend erforderlich ist, da es ja auch gelöscht werden kann.
            ModelState.Clear();
            TryValidateModel(model);
            if (!ModelState.IsValid)
            {
                trackEvents.TrackError("Invalid model state in PlaceDatabaseController.Create", new Dictionary<string, object>
                {
                    { "ModelStateErrors", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) }
                });
                return View(model);
            }

            (int statusCode, string statusMessage, int id) = processPlace.Insert(model);

            if (id > 0)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
        }

        [HandleStatus]
        public ActionResult Edit(int id)
        {
            Place? existingPlace = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [id] }).FirstOrDefault();

            if (existingPlace == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" });
            }

            PlaceEditDTO editDTO = new()
            {
                PlaceID = existingPlace.PlaceID,
                FurtherSpecs = existingPlace.FurtherSpecs,
                WikipediaUrl = existingPlace.WikipediaUrl,
                ToponymyList = [.. existingPlace.PlaceNToponymyList.Select(pnt =>  new PlaceNToponymyEditDTO
                {
                    PlaceNToponymyID = pnt.PlaceNToponymyID,
                    Name = pnt.Toponymy.ToponymyName,
                    IsCurrentName = pnt.IsCurrentName
                })]
            };
            ViewData["ConnectedPlaces"] = existingPlace.ConnectedPlaces.Select(cp => new Place
            {
                PlaceID = cp.PlaceID,
                PlaceNToponymyList = cp.PlaceNToponymyList,
                FurtherSpecs = cp.FurtherSpecs
            }).ToList();
            return View(editDTO);
        }
        [HttpPost]
        public IActionResult Edit(PlaceEditDTO model)
        {
            // 1 Model bereinigen
            model.ToponymyList = [.. model.ToponymyList.Where(x => !string.IsNullOrWhiteSpace(x.Name))];
            // 2 ModelState neu validieren, da sonst bei leeren Feldern der Fehler
            // "The ToName field is required." angezeigt wird, obwohl das Feld ja nicht zwingend erforderlich ist, da es ja auch gelöscht werden kann.
            ModelState.Clear();
            TryValidateModel(model);
            if (!ModelState.IsValid)
            {
                trackEvents.TrackError("Invalid model state in PlaceDatabaseController.Edit", new Dictionary<string, object>
                {
                    { "ModelStateErrors", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) }
                });
                return View(model);
            }
            (int statusCode, string statusMessage, int id) = processPlace.Update(model);
            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        public ActionResult Delete(int id)
        {
            Place? existingPlace = processPlace
                .GetListWithPredicate(new PlaceSearchParameterModel { PlaceID = [id] }).FirstOrDefault();
            return existingPlace == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_Place_NotFound" })
                : View(existingPlace);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int placeId)
        {
            if (placeId <= 0)
            {
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });
            }
            (int statusCode, string statusMessage) = processPlace.Delete(placeId);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
