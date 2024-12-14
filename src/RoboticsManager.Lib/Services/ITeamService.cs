using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services
{
    public interface ITeamService
    {
        Task<IEnumerable<Team>> GetAllTeamsAsync();
        Task<Team> GetTeamByIdAsync(Guid id);
        Task<Team> CreateTeamAsync(Team team);
        Task<Team> UpdateTeamAsync(Team team);
        Task DeleteTeamAsync(Guid id);
        Task<IEnumerable<Team>> GetLeaderboardAsync();
        Task<Team> AwardPointsAsync(Guid teamId, int points, string reason);
        Task<IEnumerable<ChallengeCompletion>> GetTeamCompletionsAsync(Guid teamId);
        Task<bool> HasCompletedChallengeAsync(Guid teamId, Guid challengeId);
    }
}
