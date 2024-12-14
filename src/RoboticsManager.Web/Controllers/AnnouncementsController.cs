using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboticsManager.Lib.Models;
using RoboticsManager.Lib.Services;

namespace RoboticsManager.Web.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncementsController> _logger;

        public AnnouncementsController(
            IAnnouncementService announcementService,
            ILogger<AnnouncementsController> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        // GET: Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            return View(announcements);
        }

        // GET: Announcements/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Body,Priority,IsVisible")] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _announcementService.CreateAnnouncementAsync(announcement);
                    TempData["Success"] = "Announcement created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(announcement);
        }

        // GET: Announcements/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Body,Priority,IsVisible")] Announcement announcement)
        {
            if (id != announcement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _announcementService.UpdateAnnouncementAsync(announcement);
                    TempData["Success"] = "Announcement updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    if (!await AnnouncementExists(announcement.Id))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(announcement);
        }

        // GET: Announcements/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            await _announcementService.DeleteAnnouncementAsync(id);
            TempData["Success"] = "Announcement deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Announcements/ToggleVisibility/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(Guid id)
        {
            try
            {
                var announcement = await _announcementService.ToggleVisibilityAsync(id);
                var status = announcement.IsVisible ? "visible" : "hidden";
                TempData["Success"] = $"Announcement is now {status}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error toggling visibility: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Announcements/UpdateRenderedContent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRenderedContent(Guid id)
        {
            try
            {
                await _announcementService.UpdateRenderedContentAsync(id);
                TempData["Success"] = "Announcement content updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating content: {ex.Message}";
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
