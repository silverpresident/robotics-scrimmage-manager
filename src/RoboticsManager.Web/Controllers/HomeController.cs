using Microsoft.AspNetCore.Mvc;
using RoboticsManager.Lib.Services;
using RoboticsManager.Web.Models;

namespace RoboticsManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly IChallengeService _challengeService;
        private readonly IAnnouncementService _announcementService;
        private readonly IUpdateService _updateService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ITeamService teamService,
            IChallengeService challengeService,
            IAnnouncementService announcementService,
            IUpdateService updateService,
            ILogger<HomeController> logger)
        {
            _teamService = teamService;
            _challengeService = challengeService;
            _announcementService = announcementService;
            _updateService = updateService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var teams = await _teamService.GetAllTeamsAsync();
                var announcements = await _announcementService.GetVisibleAnnouncementsAsync();
                var challenges = await _challengeService.GetAllChallengesAsync();
                var updates = await _updateService.GetRecentUpdatesAsync(10); // Get last 10 updates

                var viewModel = new HomeViewModel(teams, announcements, challenges, updates);
                viewModel.GenerateTimeline();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                throw;
            }
        }

        // Partial view actions for real-time updates
        [HttpGet]
        public async Task<IActionResult> LeaderboardPartial()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return PartialView("_LeaderboardPartial", teams.OrderByDescending(t => t.TotalPoints));
        }

        [HttpGet]
        public async Task<IActionResult> AnnouncementsPartial()
        {
            var announcements = await _announcementService.GetVisibleAnnouncementsAsync();
            return PartialView("_AnnouncementsPartial", announcements.OrderByDescending(a => a.CreatedAt));
        }

        [HttpGet]
        public async Task<IActionResult> TimelinePartial()
        {
            var announcements = await _announcementService.GetVisibleAnnouncementsAsync();
            var updates = await _updateService.GetRecentUpdatesAsync(10);

            var viewModel = new HomeViewModel(
                new List<Lib.Models.Team>(),
                announcements,
                new List<Lib.Models.Challenge>(),
                updates
            );
            viewModel.GenerateTimeline();

            return PartialView("_TimelinePartial", viewModel.Timeline);
        }

        [HttpGet]
        public async Task<IActionResult> StatsPartial()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            var challenges = await _challengeService.GetAllChallengesAsync();
            var completions = await _challengeService.GetChallengeCompletionStatsAsync();

            var viewModel = new HomeViewModel
            {
                TotalTeams = teams.Count(),
                TotalChallenges = challenges.Count(),
                TotalCompletions = completions.Sum(c => c.Value),
                TotalPoints = teams.Sum(t => t.TotalPoints)
            };

            return PartialView("_StatsPartial", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel(HttpContext.TraceIdentifier));
        }
    }
}
