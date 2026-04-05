using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCompetitionManager.Data;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Controllers
{
    [Authorize]
    public class RegistrationsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRegistrationsService _registrationsService;

        public RegistrationsController(
            UserManager<IdentityUser> userManager,
            IRegistrationsService registrationsService)
        {
            _userManager = userManager;
            _registrationsService = registrationsService;
        }

        public async Task<IActionResult> Join(int competitionId)
        {
            if (User.IsInRole(SeedData.AdminRole))
                return Forbid();

            var userId = _userManager.GetUserId(User)!;
            var result = await _registrationsService.JoinCompetitionAsync(competitionId, userId);

            if (!result.Success)
            {
                if (result.ErrorCode == "CompetitionNotFound")
                    return NotFound();

                if (result.ErrorCode == "MissingTeam")
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(
                        "Create",
                        "Team",
                        new { returnUrl = Url.Action("Competitions", "Home") });
                }

                TempData["Error"] = result.Message;
                return RedirectToAction("Competitions", "Home");
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Competitions", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unjoin(int competitionId)
        {
            if (User.IsInRole(SeedData.AdminRole))
                return Forbid();

            var userId = _userManager.GetUserId(User)!;
            var result = await _registrationsService.UnjoinCompetitionAsync(competitionId, userId);

            if (!result.Success)
            {
                if (result.ErrorCode == "CompetitionNotFound")
                    return NotFound();

                if (result.ErrorCode == "MissingTeam")
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Create", "Team");
                }

                TempData["Error"] = result.Message;
                return RedirectToAction("Competitions", "Home");
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Competitions", "Home");
        }
    }
}