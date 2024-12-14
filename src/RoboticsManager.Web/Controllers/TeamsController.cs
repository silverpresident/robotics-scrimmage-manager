using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RoboticsManager.Lib.Models;
using RoboticsManager.Lib.Services;
using RoboticsManager.Lib.Hubs;
using RoboticsManager.Lib.Extensions;

namespace RoboticsManager.Web.Controllers
{
    [Authorize]
    public class TeamsController : BaseController
    {
        private readonly ITeamService _teamService;
        private readonly IChallengeService _challengeService;
        private readonly IHubContext<UpdateHub> _hubContext;

        public TeamsController(
            ITeamService teamService,
            IChallengeService challengeService,
            IHubContext<UpdateHub> hubContext,
            UserManager<ApplicationUser> userManager,
            ILogger<TeamsController> logger)
            : base(userManager, logger)
        {
            _teamService = teamService;
            _challengeService = challengeService;
            _hubContext = hubContext;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            try
            {
                var teams = await _teamService.GetAllTeamsAsync();
                return View(teams);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Index));
            }
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
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
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Details));
            }
        }

        // GET: Teams/Create
        [Authorize(Policy = "RequireJudge")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Create([Bind("Name,TeamNo,School,Color,LogoUrl")] Team team)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _teamService.CreateTeamAsync(team);
                    await _hubContext.NotifyTeamCreated(team);
                    await LogUserAction("CreateTeam", $"Created team: {team.Name}");
                    AddSuccessMessage($"Team '{team.Name}' was created successfully.");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(team);
        }

        // GET: Teams/Edit/5
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(id);
                if (team == null)
                {
                    return NotFound();
                }

                if (!await UserCanEditTeam(id))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                return View(team);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Edit));
            }
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,TeamNo,School,Color,LogoUrl,TotalPoints")] Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (!await UserCanEditTeam(id))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _teamService.UpdateTeamAsync(team);
                    await _hubContext.NotifyTeamUpdated(team);
                    await LogUserAction("UpdateTeam", $"Updated team: {team.Name}");
                    AddSuccessMessage($"Team '{team.Name}' was updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                if (!await TeamExists(team.Id))
                {
                    return NotFound();
                }
                ModelState.AddModelError("", ex.Message);
            }
            return View(team);
        }

        // GET: Teams/Delete/5
        [Authorize(Policy = "RequireAdministrator")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(id);
                if (team == null)
                {
                    return NotFound();
                }

                return View(team);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Delete));
            }
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdministrator")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(id);
                if (team == null)
                {
                    return NotFound();
                }

                await _teamService.DeleteTeamAsync(id);
                await LogUserAction("DeleteTeam", $"Deleted team: {team.Name}");
                AddSuccessMessage($"Team '{team.Name}' was deleted successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Index));
            }
        }

        // POST: Teams/AwardPoints/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> AwardPoints(Guid id, int points, string reason)
        {
            try
            {
                if (!await UserCanAwardPoints())
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    AddErrorMessage("A reason must be provided for awarding points.");
                    return RedirectToAction(nameof(Details), new { id });
                }

                var team = await _teamService.GetTeamByIdAsync(id);
                if (team == null)
                {
                    return NotFound();
                }

                await _teamService.AwardPointsAsync(id, points, reason);
                await _hubContext.NotifyTeamPointsUpdated(team);
                await LogUserAction("AwardPoints", $"Awarded {points} points to team {team.Name}. Reason: {reason}");
                AddSuccessMessage($"{points} points awarded successfully.");
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error awarding points: {ex.Message}");
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> TeamExists(Guid id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            return team != null;
        }
    }
}
