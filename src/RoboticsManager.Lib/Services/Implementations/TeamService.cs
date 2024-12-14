using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUpdateService _updateService;

        public TeamService(ApplicationDbContext context, IUpdateService updateService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
        }

        public async Task<IEnumerable<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Include(t => t.CompletedChallenges)
                .OrderBy(t => t.TeamNo)
                .ToListAsync();
        }

        public async Task<Team> GetTeamByIdAsync(Guid id)
        {
            return await _context.Teams
                .Include(t => t.CompletedChallenges)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team> CreateTeamAsync(Team team)
        {
            if (team == null) throw new ArgumentNullException(nameof(team));

            // Validate team number uniqueness
            if (await _context.Teams.AnyAsync(t => t.TeamNo == team.TeamNo))
            {
                throw new InvalidOperationException($"Team number {team.TeamNo} is already in use.");
            }

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateTeamUpdateAsync(team, UpdateType.TeamCreated, 
                $"New team '{team.Name}' ({team.TeamNo}) from {team.School} has joined the competition!");

            return team;
        }

        public async Task<Team> UpdateTeamAsync(Team team)
        {
            if (team == null) throw new ArgumentNullException(nameof(team));

            var existingTeam = await _context.Teams.FindAsync(team.Id);
            if (existingTeam == null)
            {
                throw new InvalidOperationException($"Team with ID {team.Id} not found.");
            }

            // Check team number uniqueness if changed
            if (team.TeamNo != existingTeam.TeamNo && 
                await _context.Teams.AnyAsync(t => t.TeamNo == team.TeamNo))
            {
                throw new InvalidOperationException($"Team number {team.TeamNo} is already in use.");
            }

            // Update properties
            existingTeam.Name = team.Name;
            existingTeam.TeamNo = team.TeamNo;
            existingTeam.School = team.School;
            existingTeam.Color = team.Color;
            existingTeam.LogoUrl = team.LogoUrl;

            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateTeamUpdateAsync(existingTeam, UpdateType.TeamUpdated, 
                $"Team '{existingTeam.Name}' ({existingTeam.TeamNo}) has been updated.");

            return existingTeam;
        }

        public async Task DeleteTeamAsync(Guid id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                throw new InvalidOperationException($"Team with ID {id} not found.");
            }

            // Store team info for update message
            var teamInfo = $"'{team.Name}' ({team.TeamNo})";

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateTeamUpdateAsync(team, UpdateType.TeamDeleted, 
                $"Team {teamInfo} has been removed from the competition.");
        }

        public async Task<IEnumerable<Team>> GetLeaderboardAsync()
        {
            return await _context.Teams
                .OrderByDescending(t => t.TotalPoints)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Team> AwardPointsAsync(Guid teamId, int points, string reason)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                throw new InvalidOperationException($"Team with ID {teamId} not found.");
            }

            team.TotalPoints += points;
            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateTeamUpdateAsync(team, UpdateType.PointsAwarded, 
                $"Team '{team.Name}' ({team.TeamNo}) awarded {points} points for {reason}. New total: {team.TotalPoints}");

            // Broadcast leaderboard update
            var leaderboard = await GetLeaderboardAsync();
            await _updateService.BroadcastLeaderboardUpdateAsync(leaderboard);

            return team;
        }

        public async Task<IEnumerable<ChallengeCompletion>> GetTeamCompletionsAsync(Guid teamId)
        {
            return await _context.ChallengeCompletions
                .Include(cc => cc.Challenge)
                .Where(cc => cc.TeamId == teamId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasCompletedChallengeAsync(Guid teamId, Guid challengeId)
        {
            return await _context.ChallengeCompletions
                .AnyAsync(cc => cc.TeamId == teamId && cc.ChallengeId == challengeId);
        }
    }
}
