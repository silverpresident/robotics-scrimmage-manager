using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services
{
    public interface IChallengeService
    {
        Task<Challenge> CreateChallengeAsync(Challenge challenge);
        Task<Challenge> UpdateChallengeAsync(Challenge challenge);
        Task DeleteChallengeAsync(Guid id);
        Task<Challenge?> GetChallengeByIdAsync(Guid id);
        Task<IEnumerable<Challenge>> GetAllChallengesAsync();
        Task<IEnumerable<Challenge>> GetAvailableUniqueChallengesAsync();
        Task<ChallengeCompletion> RecordCompletionAsync(Guid challengeId, Guid teamId, string notes);
        Task RemoveCompletionAsync(Guid challengeId, Guid teamId);
        Task<IEnumerable<ChallengeCompletion>> GetCompletionsForChallengeAsync(Guid challengeId);
        Task<IEnumerable<Challenge>> GetCompletedChallengesForTeamAsync(Guid teamId);
        Task<Dictionary<Challenge, int>> GetChallengeCompletionStatsAsync();
    }
}
