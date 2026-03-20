using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
using QuizCompetitionManager.Helpers;
using System.Diagnostics;

namespace QuizCompetitionManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Competitions()
        {
            var comps = await _db.Competitions
                .OrderBy(c => c.Status)
                .ThenByDescending(c => c.StartDateTime)
                .ToListAsync();

            var vm = new PublicCompetitionsVM
            {
                Competitions = comps,
                CanJoinOrUnjoin = User.Identity?.IsAuthenticated == true && !User.IsInRole(SeedData.AdminRole)
            };

            if (vm.CanJoinOrUnjoin)
            {
                var userId = _userManager.GetUserId(User)!;

                var team = await _db.Teams.FirstOrDefaultAsync(t => t.OwnerUserId == userId);
                if (team != null)
                {
                    var joined = await _db.CompetitionRegistrations
                        .Where(r => r.TeamId == team.Id)
                        .Select(r => r.CompetitionId)
                        .ToListAsync();

                    vm.JoinedCompetitionIds = joined.ToHashSet();
                }
            }

            return View(vm);
        }

        public IActionResult Rules()
        {
            return View();
        }

        public async Task<IActionResult> CompetitionDetails(int id, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            var comp = await _db.Competitions
        .FirstOrDefaultAsync(c => c.Id == id);

            if (comp == null) return NotFound();

            var vm = new CompetitionDetailsVM
            {
                Competition = comp
            };

            if (comp.Status != CompetitionStatus.Planned)
            {
                var regs = await _db.CompetitionRegistrations
                    .Where(r => r.CompetitionId == id)
                    .Include(r => r.Team)
                    .Include(r => r.RoundScores)
                    .ToListAsync();

                var ranked = RankingHelper.BuildRanking(regs, comp.RoundsCount);

                vm.Ranking = ranked.Select(r => new RankingRowVM
                {
                    TeamName = r.TeamName,
                    TotalPoints = r.TotalPoints
                }).ToList();
            }

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
