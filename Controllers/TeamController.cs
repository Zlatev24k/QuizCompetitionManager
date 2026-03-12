using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;

namespace QuizCompetitionManager.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public TeamController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var team = await _db.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.OwnerUserId == userId);

            if (team == null)
                return RedirectToAction(nameof(Create));

            return View(team);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Името на отбора е задължително.");
                return View();
            }

            if (name.Length > 80)
            {
                ModelState.AddModelError("", "Името на отбора е твърде дълго (макс. 80 символа).");
                return View();
            }

            var userId = _userManager.GetUserId(User)!;

            var existing = await _db.Teams.AnyAsync(t => t.OwnerUserId == userId);
            if (existing)
                return RedirectToAction(nameof(Index));

            var nameExists = await _db.Teams.AnyAsync(t => t.Name.ToLower() == name.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("", "Вече съществува отбор с това име.");
                return View();
            }

            var team = new Team
            {
                Name = name,
                OwnerUserId = userId
            };

            _db.Teams.Add(team);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(string fullName)
        {
            fullName = (fullName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Името на члена е задължително.";
                return RedirectToAction(nameof(Index));
            }

            if (fullName.Length > 80)
            {
                TempData["Error"] = "Името е твърде дълго (макс. 80 символа).";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User)!;

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);
            if (team == null) return RedirectToAction(nameof(Create));

            _db.TeamMembers.Add(new TeamMember
            {
                TeamId = team.Id,
                FullName = fullName
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var member = await _db.TeamMembers
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null) return NotFound();

            if (member.Team == null || member.Team.OwnerUserId != userId)
                return Forbid();

            _db.TeamMembers.Remove(member);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditName()
        {
            var userId = _userManager.GetUserId(User)!;

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);
            if (team == null)
                return RedirectToAction(nameof(Create));

            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditName(int id, string name)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Името на отбора е задължително.");
            }
            else if (name.Length > 80)
            {
                ModelState.AddModelError("", "Името на отбора е твърде дълго (макс. 80 символа).");
            }

            var userId = _userManager.GetUserId(User)!;

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId);
            if (team == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(team);

            var nameExists = await _db.Teams.AnyAsync(t => t.Id != id && t.Name.ToLower() == name.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("", "Вече съществува отбор с това име.");
                team.Name = name;
                return View(team);
            }

            team.Name = name;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Името на отбора беше обновено успешно.";
            return RedirectToAction(nameof(Index));
        }
    }
}