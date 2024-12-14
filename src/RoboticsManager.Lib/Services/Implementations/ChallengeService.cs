using Microsoft.EntityFrameworkCore;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Services.Implementations
{
    public class ChallengeService : IChallengeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUpdateService _updateService;
        private readonly ITeamService _teamService;

        public ChallengeService(
            ApplicationDbContext context,
            IUpdateService updateService,
            ITeamService teamService)
        {
            _context = context;
            _updateService = updateService;
            _teamService = teamService;
        }

        public async Task<Challenge> CreateChallengeAsync(Challenge challenge)
        {
            if (await _context.Challenges.AnyAsync(c => c.Name == challenge.Name))
            {
                throw new InvalidOperationException($"A challenge with name '{challenge.Name}' already exists.");
            }

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            await _updateService.CreateChallengeUpdateAsync(
                challenge,
                UpdateType.ChallengeCreated,
                $"New challenge '{challenge.Name}' created"
            );

            return challenge;
        }

        public async Task<Challenge> UpdateChallengeAsync(Challenge challenge)
        {
            var existingChallenge = await _context.Challenges
                .FirstOrDefaultAsync(c => c.Id == challenge.Id);

            if (existingChallenge == null)
            {
                throw new InvalidOperationException($"Challenge not found.");
            }

            if (await _context.Challenges.AnyAsync(c => c.Name == challenge.Name && c.Id != challenge.Id))
            {
                throw new InvalidOperationException($"A challenge with name '{challenge.Name}' already exists.");
            }

            existingChallenge.Name = challenge.Name;
            existingChallenge.Description = challenge.Description;
            existingChallenge.Points = challenge.Points;
            existingChallenge.IsUnique = challenge.IsUnique;

            await _context.SaveChangesAsync();

            await _updateService.CreateChallengeUpdateAsync(
                challenge,
                UpdateType.ChallengeUpdated,
                $"Challenge '{challenge.Name}' updated"
            );

            return existingChallenge;
        }

        public async Task DeleteChallengeAsync(Guid id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Completions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
            {
                throw new InvalidOperationException($"Challenge not found.");
            }

            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            await _updateService.CreateChallengeUpdateAsync(
                challenge,
                UpdateType.ChallengeDeleted,
                $"Challenge '{challenge.Name}' deleted"
            );
        }

        public async Task<Challenge?> GetChallengeByIdAsync(Guid id)
        {
            return await _context.Challenges
                .Include(c => c.Completions)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Challenge>> GetAllChallengesAsync()
        {
            return await _context.Challenges
                .Include(c => c.Completions)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Challenge>> GetAvailableUniqueChallengesAsync()
        {
            return await _context.Challenges
                .Include(c => c.Completions)
                .Where(c => c.IsUnique && !c.Completions.Any())
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ChallengeCompletion> RecordCompletionAsync(Guid challengeId, Guid teamId, string notes)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Completions)
                .FirstOrDefaultAsync(c => c.Id == challengeId);

            if (challenge == null)
            {
                throw new InvalidOperationException("Challenge not found.");
            }

            if (await _teamService.HasCompletedChallengeAsync(teamId, challengeId))
            {
                throw new InvalidOperationException("Team has already completed this challenge.");
            }

            if (challenge.IsUnique && challenge.Completions.Any())
            {
                throw new InvalidOperationException("This unique challenge has already been completed by another team.");
            }

            var completion = new ChallengeCompletion
            {
                TeamId = teamId,
                ChallengeId = challengeId,
                PointsAwarded = challenge.Points,
                Notes = notes
            };

            _context.ChallengeCompletions.Add(completion);
            await _context.SaveChangesAsync();

            // Award points to the team
            await _teamService.AwardPointsAsync(
                teamId,
                challenge.Points,
                $"Completed challenge: {challenge.Name}"
            );

            return completion;
        }

        public async Task RemoveCompletionAsync(Guid challengeId, Guid teamId)
        {
            var completion = await _context.ChallengeCompletions
                .Include(cc => cc.Challenge)
                .FirstOrDefaultAsync(cc => cc.ChallengeId == challengeId && cc.TeamId == teamId);

            if (completion == null)
            {
                throw new InvalidOperationException("Challenge completion not found.");
            }

            _context.ChallengeCompletions.Remove(completion);
            await _context.SaveChangesAsync();

            // Deduct points from the team
            await _teamService.AwardPointsAsync(
                teamId,
                -completion.PointsAwarded,
                $"Removed completion of challenge: {completion.Challenge.Name}"
            );
        }

        public async Task<IEnumerable<ChallengeCompletion>> GetCompletionsForChallengeAsync(Guid challengeId)
        {
            return await _context.ChallengeCompletions
                .Include(cc => cc.Team)
                .Include(cc => cc.Challenge)
                .Where(cc => cc.ChallengeId == challengeId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Challenge>> GetCompletedChallengesForTeamAsync(Guid teamId)
        {
            return await _context.ChallengeCompletions
                .Include(cc => cc.Challenge)
                .Where(cc => cc.TeamId == teamId)
                .Select(cc => cc.Challenge)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Dictionary<Challenge, int>> GetChallengeCompletionStatsAsync()
        {
            var challenges = await _context.Challenges
                .Include(c => c.Completions)
                .ToListAsync();

            return challenges.ToDictionary(
                c => c,
                c => c.Completions.Count()
            );
        }
    }
}
