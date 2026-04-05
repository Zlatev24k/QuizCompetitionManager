using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Helpers;
using QuizCompetitionManager.Models;
using QuizCompetitionManager.Models.ViewModels;
using QuizCompetitionManager.Services.Interfaces;
using System.Diagnostics;

namespace QuizCompetitionManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHomeService _homeService;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            IHomeService homeService)
        {
            _logger = logger;
            _userManager = userManager;
            _homeService = homeService;
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
            var canJoinOrUnjoin =
                User.Identity?.IsAuthenticated == true &&
                !User.IsInRole(SeedData.AdminRole);

            string? userId = null;

            if (canJoinOrUnjoin)
            {
                userId = _userManager.GetUserId(User);
            }

            var vm = await _homeService.GetPublicCompetitionsAsync(canJoinOrUnjoin, userId);

            return View(vm);
        }

        public IActionResult Rules()
        {
            return View();
        }

        public async Task<IActionResult> CompetitionDetails(int id, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            var vm = await _homeService.GetCompetitionDetailsAsync(id);

            if (vm == null)
                return NotFound();

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}