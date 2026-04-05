using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Controllers
{
    [Authorize(Roles = SeedData.AdminRole)]
    public class CompetitionsController : Controller
    {
        private readonly ICompetitionsService _competitionsService;

        public CompetitionsController(ICompetitionsService competitionsService)
        {
            _competitionsService = competitionsService;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            var comps = await _competitionsService.GetAllAsync();
            return View(comps);
        }

        // CREATE
        public async Task<IActionResult> Create()
        {
            var model = await _competitionsService.GetCreateModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Competition model)
        {
            if (model.Status != CompetitionStatus.Finished && model.StartDateTime < DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.StartDateTime),
                    "Не може да създадеш състезание със задна дата, освен ако статусът не е 'Приключило'.");
            }

            if (!ModelState.IsValid)
                return View(model);

            await _competitionsService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var comp = await _competitionsService.GetByIdAsync(id);
            if (comp == null) return NotFound();

            return View(comp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Competition model)
        {
            if (id != model.Id)
                return BadRequest();

            if (model.Status != CompetitionStatus.Finished && model.StartDateTime < DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.StartDateTime),
                    "Не може да зададеш дата в миналото, освен ако статусът не е 'Приключило'.");
            }

            if (!ModelState.IsValid)
                return View(model);

            await _competitionsService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _competitionsService.GetDetailsAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var comp = await _competitionsService.GetByIdAsync(id);
            if (comp == null) return NotFound();

            return View(comp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comp = await _competitionsService.GetByIdAsync(id);
            if (comp == null) return NotFound();

            await _competitionsService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // START
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var comp = await _competitionsService.GetByIdAsync(id);
            if (comp == null) return NotFound();

            var result = await _competitionsService.StartCompetitionAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // MANAGE
        public async Task<IActionResult> Manage(int id)
        {
            var vm = await _competitionsService.GetManageDataAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // SAVE ALL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAll(CompetitionManageVM vm)
        {
            var comp = await _competitionsService.GetByIdAsync(vm.CompetitionId);
            if (comp == null) return NotFound();

            if (comp.Status != CompetitionStatus.Active)
            {
                TempData["Error"] = "Точки могат да се въвеждат само когато състезанието е активно.";
                return RedirectToAction(nameof(Manage), new { id = vm.CompetitionId });
            }

            await _competitionsService.SaveAllScoresAsync(vm);

            TempData["Success"] = "Точките са записани успешно.";
            return RedirectToAction(nameof(Manage), new { id = vm.CompetitionId });
        }

        // FINISH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(int id)
        {
            var comp = await _competitionsService.GetByIdAsync(id);
            if (comp == null) return NotFound();

            var result = await _competitionsService.FinishCompetitionAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Manage), new { id });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Manage), new { id });
        }

        // REMOVE TEAM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTeam(int competitionId, int registrationId)
        {
            var comp = await _competitionsService.GetByIdAsync(competitionId);
            if (comp == null) return NotFound();

            var result = await _competitionsService.RemoveTeamAsync(competitionId, registrationId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Manage), new { id = competitionId });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Manage), new { id = competitionId });
        }
    }
}