using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Hubs;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services.Implementations
{
    public class UpdateService : IUpdateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<UpdateHub, IUpdateClient> _hubContext;

        public UpdateService(
            ApplicationDbContext context,
            IHubContext<UpdateHub, IUpdateClient> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
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
                Type = type,
                Description = description,
                Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null
            };

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();

            await BroadcastUpdateAsync(update);

            return update;
        }

        public async Task<Update> CreateTeamUpdateAsync(Team team, UpdateType type, string description)
        {
            var update = new Update
            {
                Type = type,
                Description = description,
                TeamId = team.Id,
                Metadata = JsonSerializer.Serialize(new
                {
                    team.Name,
                    team.TeamNo,
                    team.School,
                    team.TotalPoints
                })
            };

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();

            await BroadcastUpdateAsync(update);
            await BroadcastTeamUpdateAsync(team);

            return update;
        }

        public async Task<Update> CreateChallengeUpdateAsync(Challenge challenge, UpdateType type, string description)
        {
            var update = new Update
            {
                Type = type,
                Description = description,
                ChallengeId = challenge.Id,
                Metadata = JsonSerializer.Serialize(new
                {
                    challenge.Name,
                    challenge.Points,
                    challenge.IsUnique
                })
            };

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();

            await BroadcastUpdateAsync(update);
            await BroadcastChallengeUpdateAsync(challenge);

            return update;
        }

        public async Task<Update> CreateAnnouncementUpdateAsync(Announcement announcement, UpdateType type, string description)
        {
            var update = new Update
            {
                Type = type,
                Description = description,
                AnnouncementId = announcement.Id,
                Metadata = JsonSerializer.Serialize(new
                {
                    announcement.Priority,
                    announcement.IsVisible
                })
            };

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();

            await BroadcastUpdateAsync(update);
            await BroadcastAnnouncementAsync(announcement);

            return update;
        }

        public async Task<Update> CreateChallengeCompletionUpdateAsync(ChallengeCompletion completion, string description)
        {
            var update = new Update
            {
                Type = UpdateType.ChallengeCompleted,
                Description = description,
                TeamId = completion.TeamId,
                ChallengeId = completion.ChallengeId,
                ChallengeCompletionId = completion.Id,
                Metadata = JsonSerializer.Serialize(new
                {
                    completion.PointsAwarded,
                    completion.Notes
                })
            };

            _context.Updates.Add(update);
            await _context.SaveChangesAsync();

            await BroadcastUpdateAsync(update);

            return update;
        }

        public async Task BroadcastUpdateAsync(Update update)
        {
            await _hubContext.Clients.All.ReceiveUpdate(update);
            await MarkUpdateAsBroadcastAsync(update.Id);
        }

        public async Task BroadcastTeamUpdateAsync(Team team)
        {
            await _hubContext.Clients.All.ReceiveTeamUpdate(team);
        }

        public async Task BroadcastAnnouncementAsync(Announcement announcement)
        {
            await _hubContext.Clients.All.ReceiveAnnouncement(announcement);
        }

        public async Task BroadcastLeaderboardUpdateAsync(IEnumerable<Team> leaderboard)
        {
            await _hubContext.Clients.All.ReceiveLeaderboardUpdate(leaderboard.ToArray());
        }

        public async Task BroadcastChallengeUpdateAsync(Challenge challenge)
        {
            await _hubContext.Clients.All.ReceiveChallengeUpdate(challenge);
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
                .Where(u => u.TeamId == entityId ||
                           u.ChallengeId == entityId ||
                           u.AnnouncementId == entityId ||
                           u.ChallengeCompletionId == entityId)
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
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkUpdateAsBroadcastAsync(Guid updateId)
        {
            var update = await _context.Updates.FindAsync(updateId);
            if (update != null)
            {
                update.IsBroadcast = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupOldUpdatesAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldUpdates = await _context.Updates
                .Where(u => u.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldUpdates.Any())
            {
                _context.Updates.RemoveRange(oldUpdates);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetPendingUpdateCountAsync()
        {
            return await _context.Updates.CountAsync(u => !u.IsBroadcast);
        }
    }
}
