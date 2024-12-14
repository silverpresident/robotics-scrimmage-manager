using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetVisibleAnnouncementsAsync();
        Task<Announcement> GetAnnouncementByIdAsync(Guid id);
        Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
        Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
        Task DeleteAnnouncementAsync(Guid id);
        
        // Priority-based queries
        Task<IEnumerable<Announcement>> GetAnnouncementsByPriorityAsync(AnnouncementPriority priority);
        Task<IEnumerable<Announcement>> GetRecentAnnouncementsAsync(int count = 5);
        
        // Visibility management
        Task<Announcement> ToggleVisibilityAsync(Guid id);
        Task<int> GetVisibleAnnouncementCountAsync();
        
        // Markdown processing
        Task<string> RenderMarkdownAsync(string markdown);
        Task<Announcement> UpdateRenderedContentAsync(Guid id);
        Task UpdateAllRenderedContentAsync();
        
        // Auto-announcements for challenge completions
        Task CreateChallengeCompletionAnnouncementAsync(ChallengeCompletion completion);
        Task CreateTeamUpdateAnnouncementAsync(Team team, string action);
    }
}
