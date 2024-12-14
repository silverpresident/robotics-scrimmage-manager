using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly ILogger _logger;

        protected BaseController(
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        protected ApplicationUser? CurrentUser { get; private set; }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                CurrentUser = await _userManager.GetUserAsync(User);
                if (CurrentUser != null)
                {
                    // Add common view data
                    ViewData["CurrentUserName"] = CurrentUser.FullName;
                    ViewData["CurrentUserEmail"] = CurrentUser.Email;
                    ViewData["IsAdmin"] = await _userManager.IsInRoleAsync(CurrentUser, "Administrator");
                    ViewData["IsJudge"] = await _userManager.IsInRoleAsync(CurrentUser, "Judge");
                    ViewData["IsScorekeeper"] = await _userManager.IsInRoleAsync(CurrentUser, "Scorekeeper");
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }

        protected bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Administrator");
        }

        protected bool IsCurrentUserJudge()
        {
            return User.IsInRole("Judge") || User.IsInRole("Administrator");
        }

        protected bool IsCurrentUserScorekeeper()
        {
            return User.IsInRole("Scorekeeper") || User.IsInRole("Judge") || User.IsInRole("Administrator");
        }

        protected IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        protected void AddSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        protected void AddErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        protected IActionResult HandleError(Exception ex, string action, string controller = "Home")
        {
            _logger.LogError(ex, "Error in {Controller}/{Action}: {Message}", 
                GetType().Name, action, ex.Message);
            AddErrorMessage("An error occurred while processing your request.");
            return RedirectToAction(action, controller);
        }

        protected async Task<bool> UserCanEditTeam(Guid teamId)
        {
            if (IsCurrentUserAdmin() || IsCurrentUserJudge())
                return true;

            // Add any additional team-specific authorization logic here
            return false;
        }

        protected async Task<bool> UserCanEditChallenge(Guid challengeId)
        {
            if (IsCurrentUserAdmin())
                return true;

            if (IsCurrentUserJudge())
            {
                // Add any challenge-specific judge authorization logic here
                return true;
            }

            return false;
        }

        protected async Task<bool> UserCanEditAnnouncement(Guid announcementId)
        {
            if (IsCurrentUserAdmin())
                return true;

            if (IsCurrentUserJudge())
            {
                // Add any announcement-specific judge authorization logic here
                return true;
            }

            return false;
        }

        protected async Task<bool> UserCanAwardPoints()
        {
            return IsCurrentUserJudge() || IsCurrentUserAdmin();
        }

        protected async Task LogUserAction(string action, string details)
        {
            _logger.LogInformation(
                "User {UserEmail} performed action: {Action}. Details: {Details}",
                CurrentUser?.Email ?? "Unknown",
                action,
                details
            );
        }
    }
}
