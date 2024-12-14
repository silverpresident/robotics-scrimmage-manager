using Microsoft.AspNetCore.Mvc;
using RoboticsManager.Lib.Services;

namespace RoboticsManager.Web.Controllers
{
    public class ChallengesController : Controller
    {
        private readonly IChallengeService _challengeService;
        private readonly ITeamService _teamService;
        private readonly ILogger<ChallengesController> _logger;

        public ChallengesController(
            IChallengeService challengeService,
            ITeamService teamService,
            ILogger<ChallengesController> logger)
        {
            _challengeService = challengeService;
            _teamService = teamService;
            _logger = logger;
        }

        // GET: /Challenges
        public async Task<IActionResult> Index()
        {
            var challenges = await _challengeService.GetAllChallengesAsync();
            return View(challenges);
        }

        // GET: /Challenges/Details/5
        public async Task<IActionResult> Details(Guid id)
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
    }
}
