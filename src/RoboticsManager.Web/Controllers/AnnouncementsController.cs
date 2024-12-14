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
    public class AnnouncementsController : BaseController
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IHubContext<UpdateHub> _hubContext;

        public AnnouncementsController(
            IAnnouncementService announcementService,
            IHubContext<UpdateHub> hubContext,
            UserManager<ApplicationUser> userManager,
            ILogger<AnnouncementsController> logger)
            : base(userManager, logger)
        {
            _announcementService = announcementService;
            _hubContext = hubContext;
        }

        // GET: Announcements
        public async Task<IActionResult> Index()
        {
            try
            {
                var announcements = await _announcementService.GetAllAnnouncementsAsync();
                return View(announcements);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Index));
            }
        }

        // GET: Announcements/Create
        [Authorize(Policy = "RequireJudge")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Create([Bind("Body,Priority,IsVisible")] Announcement announcement)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _announcementService.CreateAnnouncementAsync(announcement);
                    await _hubContext.NotifyAnnouncementCreated(announcement);
                    await LogUserAction("CreateAnnouncement", $"Created announcement with priority: {announcement.Priority}");
                    AddSuccessMessage("Announcement created successfully.");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(announcement);
        }

        // GET: Announcements/Edit/5
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }

                if (!await UserCanEditAnnouncement(id))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                return View(announcement);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Edit));
            }
        }

        // POST: Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Body,Priority,IsVisible")] Announcement announcement)
        {
            if (id != announcement.Id)
            {
                return NotFound();
            }

            if (!await UserCanEditAnnouncement(id))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _announcementService.UpdateAnnouncementAsync(announcement);
                    await _hubContext.NotifyAnnouncementUpdated(announcement);
                    await LogUserAction("UpdateAnnouncement", $"Updated announcement with priority: {announcement.Priority}");
                    AddSuccessMessage("Announcement updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                if (!await AnnouncementExists(announcement.Id))
                {
                    return NotFound();
                }
                ModelState.AddModelError("", ex.Message);
            }
            return View(announcement);
        }

        // GET: Announcements/Delete/5
        [Authorize(Policy = "RequireAdministrator")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }

                return View(announcement);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Delete));
            }
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdministrator")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }

                await _announcementService.DeleteAnnouncementAsync(id);
                await LogUserAction("DeleteAnnouncement", "Deleted announcement");
                AddSuccessMessage("Announcement deleted successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Index));
            }
        }

        // POST: Announcements/ToggleVisibility/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> ToggleVisibility(Guid id)
        {
            try
            {
                if (!await UserCanEditAnnouncement(id))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                var announcement = await _announcementService.ToggleVisibilityAsync(id);
                var status = announcement.IsVisible ? "visible" : "hidden";
                await LogUserAction("ToggleAnnouncementVisibility", $"Set announcement visibility to: {status}");
                AddSuccessMessage($"Announcement is now {status}.");
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error toggling visibility: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Announcements/UpdateRenderedContent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireJudge")]
        public async Task<IActionResult> UpdateRenderedContent(Guid id)
        {
            try
            {
                if (!await UserCanEditAnnouncement(id))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                await _announcementService.UpdateRenderedContentAsync(id);
                await LogUserAction("UpdateAnnouncementContent", "Updated announcement rendered content");
                AddSuccessMessage("Announcement content updated successfully.");
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error updating content: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> AnnouncementExists(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            return announcement != null;
        }
    }
}
