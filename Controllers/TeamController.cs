using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizCompetitionManager.Services.Interfaces;

namespace QuizCompetitionManager.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService, UserManager<IdentityUser> userManager)
        {
            _teamService = teamService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var team = await _teamService.GetUserTeamAsync(userId);

            if (team == null)
                return RedirectToAction(nameof(Create));

            return View(team);
        }

        public IActionResult Create(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User)!;

            var result = await _teamService.CreateTeamAsync(userId, name);

            if (!result.Success)
            {
                if (result.ErrorCode == "InvalidName" || result.ErrorCode == "DuplicateTeamName")
                {
                    ModelState.AddModelError("", result.Message);
                    ViewBag.ReturnUrl = returnUrl;
                    return View();
                }

                if (result.ErrorCode == "TeamAlreadyExistsForUser")
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", result.Message);
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            TempData.Remove("Error");
            TempData["Success"] = result.Message;

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(string fullName)
        {
            var userId = _userManager.GetUserId(User)!;

            var result = await _teamService.AddMemberAsync(userId, fullName);

            if (!result.Success)
            {
                if (result.ErrorCode == "TeamNotFound")
                    return RedirectToAction(nameof(Create));

                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var result = await _teamService.RemoveMemberAsync(userId, id);

            if (!result.Success)
            {
                if (result.ErrorCode == "MemberNotFound")
                    return NotFound();

                if (result.ErrorCode == "Forbidden")
                    return Forbid();

                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditName()
        {
            var userId = _userManager.GetUserId(User)!;

            var result = await _teamService.GetTeamForEditAsync(userId);

            if (!result.Success)
            {
                if (result.ErrorCode == "TeamNotFound")
                    return RedirectToAction(nameof(Create));

                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditName(int id, string name)
        {
            var userId = _userManager.GetUserId(User)!;

            var result = await _teamService.EditTeamNameAsync(userId, id, name);

            if (!result.Success)
            {
                if (result.ErrorCode == "TeamNotFound")
                    return NotFound();

                var teamResult = await _teamService.GetTeamForEditAsync(userId);
                if (!teamResult.Success || teamResult.Data == null)
                    return RedirectToAction(nameof(Create));

                if (result.ErrorCode == "InvalidName" || result.ErrorCode == "DuplicateTeamName")
                {
                    ModelState.AddModelError("", result.Message);
                    teamResult.Data.Name = name;
                    return View(teamResult.Data);
                }

                ModelState.AddModelError("", result.Message);
                return View(teamResult.Data);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}