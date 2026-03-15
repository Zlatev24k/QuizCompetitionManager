using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Controllers
{
    [Authorize]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public RegistrationsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Join(int competitionId)
        {

            if (User.IsInRole(SeedData.AdminRole))
                return Forbid();

            var comp = await _db.Competitions.FirstOrDefaultAsync(c => c.Id == competitionId);
            if (comp == null) return NotFound();

            if (comp.Status != CompetitionStatus.Planned)
            {
                TempData["Error"] = "Записване е възможно само за планирани състезания.";
                return RedirectToAction("Competitions", "Home");
            }

            var userId = _userManager.GetUserId(User)!;

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);
            if (team == null)
            {
                TempData["Error"] = "За да се запишеш за състезание, първо трябва да създадеш отбор.";
                return RedirectToAction(
                    "Create",
                    "Team",
                    new { returnUrl = Url.Action("Competitions", "Home") }
);
            }

            var already = await _db.CompetitionRegistrations
                .AnyAsync(r => r.CompetitionId == competitionId && r.TeamId == team.Id);

            if (already)
            {
                TempData["Error"] = "Този отбор вече е записан за състезанието.";
                return RedirectToAction("Competitions", "Home");
            }

            _db.CompetitionRegistrations.Add(new CompetitionRegistration
            {
                CompetitionId = competitionId,
                TeamId = team.Id
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Успешно записване за състезанието!";
            return RedirectToAction("Competitions", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unjoin(int competitionId)
        {
            if (User.IsInRole(SeedData.AdminRole))
                return Forbid();

            var comp = await _db.Competitions.FirstOrDefaultAsync(c => c.Id == competitionId);
            if (comp == null) return NotFound();

            if (comp.Status != CompetitionStatus.Planned)
            {
                TempData["Error"] = "Отписване е възможно само за планирани състезания.";
                return RedirectToAction("Competitions", "Home");
            }

            var userId = _userManager.GetUserId(User)!;

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);
            if (team == null)
            {
                TempData["Error"] = "Нямаш отбор.";
                return RedirectToAction("Create", "Team");
            }

            var reg = await _db.CompetitionRegistrations
                .FirstOrDefaultAsync(r => r.CompetitionId == competitionId && r.TeamId == team.Id);

            if (reg == null)
            {
                TempData["Error"] = "Отборът не е записан за това състезание.";
                return RedirectToAction("Competitions", "Home");
            }

            _db.CompetitionRegistrations.Remove(reg);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Успешно отписване от състезанието.";
            return RedirectToAction("Competitions", "Home");
        }
    }
}
