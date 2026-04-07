using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Controllers
{
    [Authorize]
    public class ResultsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IResultsService _resultsService;

        public ResultsController(
            UserManager<IdentityUser> userManager,
            IResultsService resultsService)
        {
            _userManager = userManager;
            _resultsService = resultsService;
        }

        public async Task<IActionResult> My()
        {
            if (User.IsInRole(SeedData.AdminRole))
                return Forbid();

            var userId = _userManager.GetUserId(User)!;

            var result = await _resultsService.GetMyResultsAsync(userId);

            if (!result.Success)
            {
                if (result.ErrorCode == "MissingTeam")
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Create", "Team");
                }

                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }
    }
}