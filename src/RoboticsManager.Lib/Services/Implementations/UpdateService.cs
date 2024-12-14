using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Hubs;
using RoboticsManager.Lib.Models;
using RoboticsManager.Lib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoboticsManager.Lib.Services.Implementations
{
    public class UpdateService : IUpdateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdateService> _logger;
        private readonly IHubContext<UpdateHub, IUpdateClient> _hubContext;

        public UpdateService(ApplicationDbContext context, ILogger<UpdateService> logger, IHubContext<UpdateHub, IUpdateClient> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<Update>> GetAllUpdatesAsync()
        {
            return await _context.Updates
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<Update> GetUpdateByIdAsync(Guid id)
        {
            return await _context.Updates.FindAsync(id);
        }

        public async Task<Update> CreateUpdateAsync(UpdateType type, string description, object metadata = null)
        {
            var update = new Update
            {
                Id = Guid.NewGuid(),
                Type = type,
                Description = description,
                Metadata = metadata?.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created update: {UpdateId} - {UpdateType} - {Description}", update.Id, update.Type, update.Description);
            return update;
        }

        public async Task<Update> CreateTeamUpdateAsync(Team team, UpdateType type, string description)
        {
            var update = await CreateUpdateAsync(type, description, new { TeamId = team.Id });
            _logger.LogInformation("Created team update: {UpdateId} - {UpdateType} - {Description} - {TeamId}", update.Id, update.Type, update.Description, team.Id);
            return update;
        }

        public async Task<Update> CreateChallengeUpdateAsync(Challenge challenge, UpdateType type, string description)
        {
            var update = await CreateUpdateAsync(type, description, new { ChallengeId = challenge.Id });
            _logger.LogInformation("Created challenge update: {UpdateId} - {UpdateType} - {Description} - {ChallengeId}", update.Id, update.Type, update.Description, challenge.Id);
            return update;
        }

        public async Task<Update> CreateAnnouncementUpdateAsync(Announcement announcement, UpdateType type, string description)
        {
            var update = await CreateUpdateAsync(type, description, new { AnnouncementId = announcement.Id });
            _logger.LogInformation("Created announcement update: {UpdateId} - {UpdateType} - {Description} - {AnnouncementId}", update.Id, update.Type, update.Description, announcement.Id);
            return update;
        }

        public async Task<Update> CreateChallengeCompletionUpdateAsync(ChallengeCompletion completion, string description)
        {
            var update = await CreateUpdateAsync(UpdateType.ChallengeCompletion, description, new { ChallengeId = completion.ChallengeId, TeamId = completion.TeamId });
            _logger.LogInformation("Created challenge completion update: {UpdateId} - {UpdateType} - {Description} - {ChallengeId} - {TeamId}", update.Id, update.Type, update.Description, completion.ChallengeId, completion.TeamId);
            return update;
        }

        public async Task BroadcastUpdateAsync(Update update)
        {
            await _hubContext.Clients.All.ReceiveUpdate();
            _logger.LogInformation("Broadcasted update: {UpdateId} - {UpdateType} - {Description}", update.Id, update.Type, update.Description);
        }

        public async Task BroadcastTeamUpdateAsync(Team team)
        {
            await _hubContext.Clients.All.ReceiveTeamUpdate(team.Name);
            _logger.LogInformation("Broadcasted team update: {TeamId} - {TeamName}", team.Id, team.Name);
        }

        public async Task BroadcastAnnouncementAsync(Announcement announcement)
        {
            await _hubContext.Clients.All.ReceiveAnnouncement();
            _logger.LogInformation("Broadcasted announcement update: {AnnouncementId} - {AnnouncementBody}", announcement.Id, announcement.Body);
        }

        public async Task BroadcastLeaderboardUpdateAsync(IEnumerable<Team> leaderboard)
        {
            await _hubContext.Clients.All.ReceiveLeaderboardUpdate();
            _logger.LogInformation("Broadcasted leaderboard update");
        }

        public async Task BroadcastChallengeUpdateAsync(Challenge challenge)
        {
            await _hubContext.Clients.All.ReceiveChallengeUpdate(challenge.Name);
            _logger.LogInformation("Broadcasted challenge update: {ChallengeId} - {ChallengeName}", challenge.Id, challenge.Name);
        }

        public async Task<IEnumerable<Update>> GetUpdatesByTypeAsync(UpdateType type)
        {
            return await _context.Updates
                .Where(u => u.Type == type)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> GetUpdatesByEntityAsync(Guid entityId)
        {
            return await _context.Updates
                .Where(u => u.Metadata.Contains(entityId.ToString()))
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> GetRecentUpdatesAsync(int count = 10)
        {
            return await _context.Updates
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Update>> GetUnbroadcastUpdatesAsync()
        {
            return await _context.Updates
                .Where(u => !u.IsBroadcast)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkUpdateAsBroadcastAsync(Guid updateId)
        {
            var update = await _context.Updates.FindAsync(updateId);
            if (update != null)
            {
                update.IsBroadcast = true;
                update.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Marked update as broadcast: {UpdateId}", update.Id);
            }
        }

        public async Task CleanupOldUpdatesAsync(int daysToKeep = 30)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
            var updatesToDelete = await _context.Updates
                .Where(u => u.CreatedAt < cutoff)
                .ToListAsync();

            _context.Updates.RemoveRange(updatesToDelete);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up old updates created before: {CutoffDate}", cutoff);
        }

        public async Task<int> GetPendingUpdateCountAsync()
        {
            return await _context.Updates.CountAsync(u => !u.IsBroadcast);
        }
    }
}
