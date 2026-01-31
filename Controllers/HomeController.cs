using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Models;
using System.Diagnostics;

namespace QuizCompetitionManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
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
                .OrderByDescending(c => c.StartDateTime)
                .ToListAsync();

            return View(comps);
        }
        public IActionResult Rules()
        {
            return View();
        }
        public async Task<IActionResult> CompetitionDetails(int id)
        {
            var comp = await _db.Competitions.FirstOrDefaultAsync(c => c.Id == id);
            if (comp == null) return NotFound();

            return View(comp);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
