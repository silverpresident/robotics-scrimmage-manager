using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoboticsManager.Lib.Models;
using RoboticsManager.Lib.Services;
using RoboticsManager.Web.Models;

namespace RoboticsManager.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ITeamService _teamService;
        private readonly IChallengeService _challengeService;
        private readonly IAnnouncementService _announcementService;
        private readonly IUpdateService _updateService;

        public HomeController(
            ITeamService teamService,
            IChallengeService challengeService,
            IAnnouncementService announcementService,
            IUpdateService updateService,
            UserManager<ApplicationUser> userManager,
            ILogger<HomeController> logger)
            : base(userManager, logger)
        {
            _teamService = teamService;
            _challengeService = challengeService;
            _announcementService = announcementService;
            _updateService = updateService;
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
                return HandleError(ex, nameof(Index));
            }
        }

        // Partial view actions for real-time updates
        [HttpGet]
        public async Task<IActionResult> LeaderboardPartial()
        {
            try
            {
                var teams = await _teamService.GetAllTeamsAsync();
                return PartialView("_LeaderboardPartial", teams.OrderByDescending(t => t.TotalPoints));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading leaderboard");
                return Content("Error loading leaderboard. Please refresh the page.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AnnouncementsPartial()
        {
            try
            {
                var announcements = await _announcementService.GetVisibleAnnouncementsAsync();
                return PartialView("_AnnouncementsPartial", announcements.OrderByDescending(a => a.CreatedAt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading announcements");
                return Content("Error loading announcements. Please refresh the page.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TimelinePartial()
        {
            try
            {
                var announcements = await _announcementService.GetVisibleAnnouncementsAsync();
                var updates = await _updateService.GetRecentUpdatesAsync(10);

                var viewModel = new HomeViewModel(
                    new List<Team>(),
                    announcements,
                    new List<Challenge>(),
                    updates
                );
                viewModel.GenerateTimeline();

                return PartialView("_TimelinePartial", viewModel.Timeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading timeline");
                return Content("Error loading timeline. Please refresh the page.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> StatsPartial()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stats");
                return Content("Error loading stats. Please refresh the page.");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel(HttpContext.TraceIdentifier));
        }
    }
}
