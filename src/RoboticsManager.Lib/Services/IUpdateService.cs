using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services
{
    public interface IUpdateService
    {
        // CRUD operations
        Task<IEnumerable<Update>> GetAllUpdatesAsync();
        Task<Update> GetUpdateByIdAsync(Guid id);
        Task<Update> CreateUpdateAsync(UpdateType type, string description, object metadata = null);
        
        // Entity-specific updates
        Task<Update> CreateTeamUpdateAsync(Team team, UpdateType type, string description);
        Task<Update> CreateChallengeUpdateAsync(Challenge challenge, UpdateType type, string description);
        Task<Update> CreateAnnouncementUpdateAsync(Announcement announcement, UpdateType type, string description);
        Task<Update> CreateChallengeCompletionUpdateAsync(ChallengeCompletion completion, string description);
        
        // Real-time notifications
        Task BroadcastUpdateAsync(Update update);
        Task BroadcastTeamUpdateAsync(Team team);
        Task BroadcastAnnouncementAsync(Announcement announcement);
        Task BroadcastLeaderboardUpdateAsync(IEnumerable<Team> leaderboard);
        Task BroadcastChallengeUpdateAsync(Challenge challenge);
        
        // Query methods
        Task<IEnumerable<Update>> GetUpdatesByTypeAsync(UpdateType type);
        Task<IEnumerable<Update>> GetUpdatesByEntityAsync(Guid entityId);
        Task<IEnumerable<Update>> GetRecentUpdatesAsync(int count = 10);
        Task<IEnumerable<Update>> GetUnbroadcastUpdatesAsync();
        
        // Maintenance
        Task MarkUpdateAsBroadcastAsync(Guid updateId);
        Task CleanupOldUpdatesAsync(int daysToKeep = 30);
        Task<int> GetPendingUpdateCountAsync();
    }
}
