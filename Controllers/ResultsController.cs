using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
using QuizCompetitionManager.Helpers;

namespace QuizCompetitionManager.Controllers
{
    [Authorize]
    public class ResultsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ResultsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> My()
        {
            if (User.IsInRole(SeedData.AdminRole))
                return Forbid();

            var userId = _userManager.GetUserId(User)!;

            var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);
            if (team == null)
            {
                TempData["Error"] = "Първо трябва да създадеш отбор.";
                return RedirectToAction("Create", "Team");
            }

            var regs = await _db.CompetitionRegistrations
                .Where(r => r.TeamId == team.Id)
                .Include(r => r.Competition)
                .Include(r => r.RoundScores)
                .OrderByDescending(r => r.Competition!.StartDateTime)
                .ToListAsync();

            var vm = new MyResultsVM
            {
                TeamName = team.Name
            };

            foreach (var reg in regs)
            {
                var comp = reg.Competition!;
                var rounds = Enumerable.Range(1, comp.RoundsCount)
                    .Select(n =>
                    {
                        var score = reg.RoundScores.FirstOrDefault(s => s.RoundNumber == n);
                        return new RoundResultVM
                        {
                            RoundNumber = n,
                            Points = score?.Points ?? 0
                        };
                    })
                    .ToList();

                var total = reg.RoundScores.Sum(s => s.Points);

                var item = new MyCompetitionResultVM
                {
                    CompetitionId = comp.Id,
                    CompetitionName = comp.Name,
                    StartDateTime = comp.StartDateTime,
                    Status = comp.Status,
                    RoundsCount = comp.RoundsCount,
                    Rounds = rounds,
                    TotalPoints = total
                };

                if (comp.Status != CompetitionStatus.Planned)
                {
                    var allRegs = await _db.CompetitionRegistrations
                        .Where(r => r.CompetitionId == comp.Id)
                        .Include(r => r.RoundScores)
                        .Include(r => r.Team)
                        .ToListAsync();

                    item.TeamsCount = allRegs.Count;

                    var ranked = RankingHelper.BuildRanking(allRegs, comp.RoundsCount);

                    if (ranked.Any(x => x.TotalPoints > 0))
                    {
                        var index = ranked.FindIndex(x => x.TeamId == team.Id); 
                        if (index >= 0)
                            item.Position = index + 1;
                    }
                }

                vm.Competitions.Add(item);
            }

            return View(vm);
        }
    }
}
