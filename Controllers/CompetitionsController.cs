using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Controllers
{
    [Authorize(Roles = SeedData.AdminRole)]
    public class CompetitionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CompetitionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            var comps = await _db.Competitions
                .OrderByDescending(c => c.StartDateTime)
                .ToListAsync();

            return View(comps);
        }

        // CREATE
        public IActionResult Create() => View(new Competition { StartDateTime = DateTime.Now, RoundsCount = 4 });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Competition model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Competitions.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);
            if (comp == null) return NotFound();
            return View(comp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Competition model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var comp = await _db.Competitions.FirstOrDefaultAsync(c => c.Id == id);
            if (comp == null) return NotFound();
            return View(comp);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);
            if (comp == null) return NotFound();
            return View(comp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);
            if (comp == null) return NotFound();

            _db.Competitions.Remove(comp);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //START
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);
            if (comp == null) return NotFound();

            if (comp.Status != CompetitionStatus.Planned)
            {
                TempData["Error"] = "Само планирано състезание може да бъде стартирано.";
                return RedirectToAction(nameof(Index));
            }

            var hasTeams = await _db.CompetitionRegistrations
                .AnyAsync(r => r.CompetitionId == id);

            if (!hasTeams)
            {
                TempData["Error"] = "Не може да стартираш състезание без записани отбори.";
                return RedirectToAction(nameof(Index));
            }

            comp.Status = CompetitionStatus.Active;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Състезанието е стартирано успешно.";
            return RedirectToAction(nameof(Index));
        }


    }
}