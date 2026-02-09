using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
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

        //MANAGE
        public async Task<IActionResult> Manage(int id)
        {
            var comp = await _db.Competitions
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comp == null) return NotFound();

            var regs = await _db.CompetitionRegistrations
                .Where(r => r.CompetitionId == id)
                .Include(r => r.Team)
                .Include(r => r.RoundScores)
                .AsNoTracking()
                .ToListAsync();

            var vm = new CompetitionManageVM
            {
                CompetitionId = comp.Id,
                CompetitionName = comp.Name,
                StartDateTime = comp.StartDateTime,
                Status = comp.Status,
                RoundsCount = comp.RoundsCount,
                Teams = regs
                    .OrderBy(r => r.Team!.Name)
                    .Select(r => new TeamScoreRowVM
                    {
                        RegistrationId = r.Id,
                        TeamName = r.Team!.Name,
                        RoundPoints = Enumerable.Range(1, comp.RoundsCount)
                            .Select(round =>
                            {
                                var existing = r.RoundScores.FirstOrDefault(s => s.RoundNumber == round);
                                return new RoundPointVM
                                {
                                    RoundNumber = round,
                                    Points = existing?.Points ?? 0
                                };
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return View(vm);
        }

        //SAVEALL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAll(CompetitionManageVM vm)
        {
            var comp = await _db.Competitions.FirstOrDefaultAsync(c => c.Id == vm.CompetitionId);
            if (comp == null) return NotFound();

            if (comp.Status != CompetitionStatus.Active)
            {
                TempData["Error"] = "Точки могат да се въвеждат само когато състезанието е активно.";
                return RedirectToAction(nameof(Manage), new { id = vm.CompetitionId });
            }

            var regIds = vm.Teams.Select(t => t.RegistrationId).ToList();

            var existingScores = await _db.RoundScores
                .Where(s => regIds.Contains(s.CompetitionRegistrationId))
                .ToListAsync();

            var scoreMap = existingScores.ToDictionary(
                s => (s.CompetitionRegistrationId, s.RoundNumber),
                s => s
            );

            foreach (var team in vm.Teams)
            {
                foreach (var rp in team.RoundPoints)
                {
                    if (rp.RoundNumber < 1 || rp.RoundNumber > comp.RoundsCount)
                        continue;

                    var points = rp.Points < 0 ? 0 : rp.Points;

                    var key = (team.RegistrationId, rp.RoundNumber);

                    if (scoreMap.TryGetValue(key, out var score))
                    {
                        score.Points = points;
                    }
                    else
                    {
                        _db.RoundScores.Add(new RoundScore
                        {
                            CompetitionRegistrationId = team.RegistrationId,
                            RoundNumber = rp.RoundNumber,
                            Points = points
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Точките са записани успешно.";
            return RedirectToAction(nameof(Manage), new { id = vm.CompetitionId });
        }

        //FINISH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(int id)
        {
            var comp = await _db.Competitions.FindAsync(id);
            if (comp == null) return NotFound();

            if (comp.Status != CompetitionStatus.Active)
            {
                TempData["Error"] = "Само активно състезание може да бъде приключено.";
                return RedirectToAction(nameof(Manage), new { id });
            }

            comp.Status = CompetitionStatus.Finished;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Състезанието е приключено и е преместено в Архив.";
            return RedirectToAction(nameof(Manage), new { id });
        }
    }
}