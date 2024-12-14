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
    public class ChallengesController : BaseController
    {
        private readonly IChallengeService _challengeService;
        private readonly ITeamService _teamService;
        private readonly IHubContext<UpdateHub> _hubContext;

        public ChallengesController(
            IChallengeService challengeService,
            ITeamService teamService,
            IHubContext<UpdateHub> hubContext,
            UserManager<ApplicationUser> userManager,
            ILogger<ChallengesController> logger)
            : base(userManager, logger)
        {
            _challengeService = challengeService;
            _teamService = teamService;
            _hubContext = hubContext;
        }

        // GET: Challenges
        public async Task<IActionResult> Index()
        {
            try
            {
                var challenges = await _challengeService.GetAllChallengesAsync();
                return View(challenges);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Index));
            }
        }

        // GET: Challenges/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var challenge = await _challengeService.GetChallengeByIdAsync(id);
                if (challenge == null)
                {
                    return NotFound();
                }

                // Get completion statistics
                var completions = await _challengeService.GetCompletionsForChallengeAsync(id);
                var teams = await _teamService.GetAllTeamsAsync();
                var completionRate = teams.Any() 
                    ? (double)completions.Count() / teams.Count() * 100 
                    : 0;

                ViewData["CompletionRate"] = completionRate;
                ViewData["CompletionCount"] = completions.Count();
                ViewData["TotalTeams"] = teams.Count();
                ViewData["IsAvailable"] = !challenge.IsUnique || !completions.Any();

                return View(challenge);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Details));
            }
        }

        // GET: Challenges/Create
        [Authorize(Policy = "RequireJudge")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Challenges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Create([Bind("Name,Description,Points,IsUnique")] Challenge challenge)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _challengeService.CreateChallengeAsync(challenge);
                    await _hubContext.NotifyChallengeCreated(challenge);
                    await LogUserAction("CreateChallenge", $"Created challenge: {challenge.Name}");
                    AddSuccessMessage($"Challenge '{challenge.Name}' was created successfully.");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(challenge);
        }

        // GET: Challenges/Edit/5
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var challenge = await _challengeService.GetChallengeByIdAsync(id);
                if (challenge == null)
                {
                    return NotFound();
                }

                if (!await UserCanEditChallenge(id))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                return View(challenge);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Edit));
            }
        }

        // POST: Challenges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description,Points,IsUnique")] Challenge challenge)
        {
            if (id != challenge.Id)
            {
                return NotFound();
            }

            if (!await UserCanEditChallenge(id))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _challengeService.UpdateChallengeAsync(challenge);
                    await _hubContext.NotifyChallengeUpdated(challenge);
                    await LogUserAction("UpdateChallenge", $"Updated challenge: {challenge.Name}");
                    AddSuccessMessage($"Challenge '{challenge.Name}' was updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(challenge);
        }

        // GET: Challenges/Delete/5
        [Authorize(Policy = "RequireAdministrator")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var challenge = await _challengeService.GetChallengeByIdAsync(id);
                if (challenge == null)
                {
                    return NotFound();
                }

                return View(challenge);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Delete));
            }
        }

        // POST: Challenges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdministrator")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var challenge = await _challengeService.GetChallengeByIdAsync(id);
                if (challenge == null)
                {
                    return NotFound();
                }

                await _challengeService.DeleteChallengeAsync(id);
                await LogUserAction("DeleteChallenge", $"Deleted challenge: {challenge.Name}");
                AddSuccessMessage($"Challenge '{challenge.Name}' was deleted successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Index));
            }
        }

        // POST: Challenges/CompleteChallenge
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> CompleteChallenge(Guid challengeId, Guid teamId, string notes)
        {
            try
            {
                if (!await UserCanAwardPoints())
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                var challenge = await _challengeService.GetChallengeByIdAsync(challengeId);
                var team = await _teamService.GetTeamByIdAsync(teamId);

                if (challenge == null || team == null)
                {
                    return NotFound();
                }

                var completion = await _challengeService.RecordCompletionAsync(challengeId, teamId, notes);
                await _hubContext.NotifyChallengeCompleted(team, challenge);
                await LogUserAction("CompleteChallenge", 
                    $"Recorded completion of challenge '{challenge.Name}' for team '{team.Name}'");
                AddSuccessMessage($"Challenge completion recorded successfully.");

                return RedirectToAction(nameof(Details), new { id = challengeId });
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error recording challenge completion: {ex.Message}");
                return RedirectToAction(nameof(Details), new { id = challengeId });
            }
        }
    }
}
