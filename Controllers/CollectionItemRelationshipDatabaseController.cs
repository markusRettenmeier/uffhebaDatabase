using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class CollectionItemRelationshipDatabaseController(IProcessCIRelationship processCIRelationship) : Controller
    {

        [HandleStatus]
        public IActionResult Index(CIRelationshipSearchParameterModel searchParameterModel)
        {
            return View(processCIRelationship.GetListWithPredicates(searchParameterModel));
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: CollectionItemRelationship/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CIRelationshipCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }
            (int statusCode, string statusMessage, int id) = processCIRelationship.Insert(createDTO);

            if (id > 0)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusMessage, statusCode });
        }

        // GET: CollectionItemRelationship/Edit/5
        [HandleStatus]
        public async Task<IActionResult> Edit(int id)
        {
            CollectionItemRelationship? ciRelationship = processCIRelationship.GetListWithPredicates(
                new CIRelationshipSearchParameterModel { CollectionItemRelationshipId = [id] }).FirstOrDefault();
            if (ciRelationship == null)
            {
                return RedirectToAction(nameof(Index), new { statusMessage = "Error_CIRelationship_NotFound" });
            }

            CIRelationshipEditDTO editDTO = new()
            {
                Id = ciRelationship.CollectionItemRelationshipId,
                Name = ciRelationship.CollectionItemRelationshipName
            };
            return View(editDTO);
        }

        // POST: CollectionItemRelationship/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CIRelationshipEditDTO editDto)
        {
            if (!ModelState.IsValid)
            {
                return View(editDto);
            }
            (int statusCode, string statusMessage, int id) = processCIRelationship.Update(editDto);

            if (statusCode == 200)
                return RedirectToAction(nameof(Edit), new { statusCode, statusMessage, id });
            else
                return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }

        // GET: CollectionItemRelationship/DeleteRangeByUserId/5
        public async Task<IActionResult> Delete(int id)
        {
            CollectionItemRelationship? ciRelationship = processCIRelationship.GetListWithPredicates(
                new CIRelationshipSearchParameterModel { CollectionItemRelationshipId = [id] }).FirstOrDefault();
            return ciRelationship == null
                ? RedirectToAction(nameof(Index), new { statusMessage = "Error_CIRelationship_NotFound" })
                : View(ciRelationship);
        }

        // POST: CollectionItemRelationship/DeleteRangeByUserId/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int CollectionItemRelationshipId)
        {
            if (CollectionItemRelationshipId <= 0)
                return RedirectToAction(nameof(Index),
                    new { statusMessage = "Error_Invalid_Id" });

            (int statusCode, string statusMessage) = processCIRelationship.Delete(CollectionItemRelationshipId);
            return RedirectToAction(nameof(Index), new { statusCode, statusMessage });
        }
    }
}
