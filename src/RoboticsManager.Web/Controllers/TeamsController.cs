using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboticsManager.Lib.Models;
using RoboticsManager.Lib.Services;

namespace RoboticsManager.Web.Controllers
{
    [Authorize]
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly IChallengeService _challengeService;
        private readonly ILogger<TeamsController> _logger;

        public TeamsController(
            ITeamService teamService,
            IChallengeService challengeService,
            ILogger<TeamsController> logger)
        {
            _teamService = teamService;
            _challengeService = challengeService;
            _logger = logger;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return View(teams);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            // Get team's completed challenges
            var completedChallenges = await _challengeService.GetCompletedChallengesForTeamAsync(id);
            ViewData["CompletedChallenges"] = completedChallenges;

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TeamNo,School,Color,LogoUrl")] Team team)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _teamService.CreateTeamAsync(team);
                    TempData["Success"] = $"Team '{team.Name}' was created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,TeamNo,School,Color,LogoUrl,TotalPoints")] Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _teamService.UpdateTeamAsync(team);
                    TempData["Success"] = $"Team '{team.Name}' was updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    if (!await TeamExists(team.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            await _teamService.DeleteTeamAsync(id);
            TempData["Success"] = $"Team '{team.Name}' was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TeamExists(Guid id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            return team != null;
        }

        // POST: Teams/AwardPoints/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AwardPoints(Guid id, int points, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A reason must be provided for awarding points.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                await _teamService.AwardPointsAsync(id, points, reason);
                TempData["Success"] = $"{points} points awarded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error awarding points: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
