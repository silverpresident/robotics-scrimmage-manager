using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Microsoft.EntityFrameworkCore;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUpdateService _updateService;
        private static readonly MarkdownPipeline _markdownPipeline;

        static AnnouncementService()
        {
            // Configure Markdig pipeline with common extensions
            _markdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmojiAndSmiley()
                .UseTaskLists()
                .Build();
        }

        public AnnouncementService(ApplicationDbContext context, IUpdateService updateService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
        }

        public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetVisibleAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => a.IsVisible)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement> GetAnnouncementByIdAsync(Guid id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
        {
            if (announcement == null) throw new ArgumentNullException(nameof(announcement));

            // Render markdown content
            announcement.RenderedBody = await RenderMarkdownAsync(announcement.Body);

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateAnnouncementUpdateAsync(announcement, UpdateType.AnnouncementCreated,
                $"New {announcement.Priority.ToString().ToLower()} announcement posted!");

            // Broadcast the announcement itself
            await _updateService.BroadcastAnnouncementAsync(announcement);

            return announcement;
        }

        public async Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
        {
            if (announcement == null) throw new ArgumentNullException(nameof(announcement));

            var existingAnnouncement = await _context.Announcements.FindAsync(announcement.Id);
            if (existingAnnouncement == null)
            {
                throw new InvalidOperationException($"Announcement with ID {announcement.Id} not found.");
            }

            // Update properties
            existingAnnouncement.Body = announcement.Body;
            existingAnnouncement.Priority = announcement.Priority;
            existingAnnouncement.IsVisible = announcement.IsVisible;

            // Re-render markdown content
            existingAnnouncement.RenderedBody = await RenderMarkdownAsync(announcement.Body);

            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateAnnouncementUpdateAsync(existingAnnouncement, UpdateType.AnnouncementUpdated,
                "Announcement has been updated.");

            // Broadcast the updated announcement
            await _updateService.BroadcastAnnouncementAsync(existingAnnouncement);

            return existingAnnouncement;
        }

        public async Task DeleteAnnouncementAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement with ID {id} not found.");
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateAnnouncementUpdateAsync(announcement, UpdateType.AnnouncementDeleted,
                "Announcement has been removed.");
        }

        public async Task<IEnumerable<Announcement>> GetAnnouncementsByPriorityAsync(AnnouncementPriority priority)
        {
            return await _context.Announcements
                .Where(a => a.Priority == priority && a.IsVisible)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetRecentAnnouncementsAsync(int count = 5)
        {
            return await _context.Announcements
                .Where(a => a.IsVisible)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Announcement> ToggleVisibilityAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement with ID {id} not found.");
            }

            announcement.IsVisible = !announcement.IsVisible;
            await _context.SaveChangesAsync();

            // Broadcast the visibility change
            await _updateService.BroadcastAnnouncementAsync(announcement);

            return announcement;
        }

        public async Task<int> GetVisibleAnnouncementCountAsync()
        {
            return await _context.Announcements.CountAsync(a => a.IsVisible);
        }

        public async Task<string> RenderMarkdownAsync(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return string.Empty;
            
            // Render markdown to HTML
            return await Task.Run(() => Markdown.ToHtml(markdown, _markdownPipeline));
        }

        public async Task<Announcement> UpdateRenderedContentAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                throw new InvalidOperationException($"Announcement with ID {id} not found.");
            }

            announcement.RenderedBody = await RenderMarkdownAsync(announcement.Body);
            await _context.SaveChangesAsync();

            return announcement;
        }

        public async Task UpdateAllRenderedContentAsync()
        {
            var announcements = await _context.Announcements.ToListAsync();
            foreach (var announcement in announcements)
            {
                announcement.RenderedBody = await RenderMarkdownAsync(announcement.Body);
            }
            await _context.SaveChangesAsync();
        }

        public async Task CreateChallengeCompletionAnnouncementAsync(ChallengeCompletion completion)
        {
            var team = await _context.Teams.FindAsync(completion.TeamId);
            var challenge = await _context.Challenges.FindAsync(completion.ChallengeId);

            if (team == null || challenge == null) return;

            var announcement = new Announcement
            {
                Body = $"🎉 Team **{team.Name}** ({team.TeamNo}) has completed the challenge **{challenge.Name}** " +
                      $"and earned {completion.PointsAwarded} points!" +
                      (challenge.IsUnique ? "\n\n*This was a unique challenge!*" : ""),
                Priority = AnnouncementPriority.Info,
                IsVisible = true
            };

            await CreateAnnouncementAsync(announcement);
        }

        public async Task CreateTeamUpdateAnnouncementAsync(Team team, string action)
        {
            var announcement = new Announcement
            {
                Body = $"📢 Team **{team.Name}** ({team.TeamNo}) from {team.School} {action}",
                Priority = AnnouncementPriority.Info,
                IsVisible = true
            };

            await CreateAnnouncementAsync(announcement);
        }
    }
}
