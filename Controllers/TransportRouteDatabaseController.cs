using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.TransportRouteDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class TransportRouteDatabaseController(IProcessPlace processPlace
        , IProcessTransportRoute processTransportRoute) : Controller
    {
        public ActionResult Index(string statusMessage, PlaceSearchParameter placeSearchParameter)
        {
            ViewData["StatusMessage"] = statusMessage;

            List<Place> placeList = [.. processPlace.GetListWithPredicate(placeSearchParameter).Where(x => x.BodyOfWater != null)];
            return View(placeList);
        }

        public ActionResult Create(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;
            return View();
        }
        public IActionResult CreateSubmit(TransportRouteOperationParameterModel model)
        {
            (int _, int _, string statusMessage) = processTransportRoute.CreateTransportRoute(model);
            return RedirectToAction(nameof(Create), new { statusMessage });
        }

        public ActionResult Edit(string statusMessage, int id)
        {
            ViewData["StatusMessage"] = statusMessage;

            PlaceSearchParameter searchParameter = new()
            {
                PlaceID = [id]
            };
            Place? existingPlace = processPlace.GetListWithPredicate(searchParameter).FirstOrDefault();
            if (existingPlace == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Transportroute nicht gefunden" });
            }

            TransportRouteOperationParameterModel transportRouteOperationParameterModel = new()
            {
                Place = existingPlace,
                TransportRoute = existingPlace.TransportRoute!,
                PlaceNToponymyList = existingPlace.PlaceNToponymyList,
                ChildPlaceList = existingPlace.ChildPlaceList
            };

            return View(transportRouteOperationParameterModel);
        }
        public IActionResult EditSubmit(TransportRouteOperationParameterModel model)
        {
            (int placeID, int _, string statusMessage) = processTransportRoute.EditTransportRoute(model);
            return RedirectToAction(nameof(Edit), new { statusMessage, id = placeID });
        }
    }
}
