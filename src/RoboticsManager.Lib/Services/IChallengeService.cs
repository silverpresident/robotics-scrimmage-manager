using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services
{
    public interface IChallengeService
    {
        Task<IEnumerable<Challenge>> GetAllChallengesAsync();
        Task<Challenge> GetChallengeByIdAsync(Guid id);
        Task<Challenge> CreateChallengeAsync(Challenge challenge);
        Task<Challenge> UpdateChallengeAsync(Challenge challenge);
        Task DeleteChallengeAsync(Guid id);
        
        // Challenge completion methods
        Task<ChallengeCompletion> RecordCompletionAsync(Guid challengeId, Guid teamId, string notes);
        Task<bool> IsUniqueCompletionAvailableAsync(Guid challengeId);
        Task<IEnumerable<ChallengeCompletion>> GetCompletionsForChallengeAsync(Guid challengeId);
        Task<IEnumerable<Team>> GetTeamsCompletedChallengeAsync(Guid challengeId);
        Task<int> GetTotalCompletionsAsync(Guid challengeId);
        Task RemoveCompletionAsync(Guid challengeId, Guid teamId);
        
        // Statistics and reporting
        Task<IDictionary<Challenge, int>> GetChallengeCompletionStatsAsync();
        Task<IEnumerable<Challenge>> GetMostPopularChallengesAsync(int count = 5);
        Task<IEnumerable<Challenge>> GetAvailableUniqueChallengesAsync();
    }
}
