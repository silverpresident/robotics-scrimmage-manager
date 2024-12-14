using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
            _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
        }

        public async Task<IEnumerable<Challenge>> GetAllChallengesAsync()
        {
            return await _context.Challenges
                .Include(c => c.Completions)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Challenge> GetChallengeByIdAsync(Guid id)
        {
            return await _context.Challenges
                .Include(c => c.Completions)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Challenge> CreateChallengeAsync(Challenge challenge)
        {
            if (challenge == null) throw new ArgumentNullException(nameof(challenge));

            // Validate challenge name uniqueness
            if (await _context.Challenges.AnyAsync(c => c.Name == challenge.Name))
            {
                throw new InvalidOperationException($"Challenge name '{challenge.Name}' is already in use.");
            }

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateChallengeUpdateAsync(challenge, UpdateType.ChallengeCreated,
                $"New challenge '{challenge.Name}' worth {challenge.Points} points has been added!");

            return challenge;
        }

        public async Task<Challenge> UpdateChallengeAsync(Challenge challenge)
        {
            if (challenge == null) throw new ArgumentNullException(nameof(challenge));

            var existingChallenge = await _context.Challenges.FindAsync(challenge.Id);
            if (existingChallenge == null)
            {
                throw new InvalidOperationException($"Challenge with ID {challenge.Id} not found.");
            }

            // Check name uniqueness if changed
            if (challenge.Name != existingChallenge.Name &&
                await _context.Challenges.AnyAsync(c => c.Name == challenge.Name))
            {
                throw new InvalidOperationException($"Challenge name '{challenge.Name}' is already in use.");
            }

            // Update properties
            existingChallenge.Name = challenge.Name;
            existingChallenge.Description = challenge.Description;
            existingChallenge.Points = challenge.Points;
            existingChallenge.IsUnique = challenge.IsUnique;

            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateChallengeUpdateAsync(existingChallenge, UpdateType.ChallengeUpdated,
                $"Challenge '{existingChallenge.Name}' has been updated.");

            return existingChallenge;
        }

        public async Task DeleteChallengeAsync(Guid id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
            {
                throw new InvalidOperationException($"Challenge with ID {id} not found.");
            }

            // Store challenge info for update message
            var challengeInfo = $"'{challenge.Name}'";

            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            // Create and broadcast update
            await _updateService.CreateChallengeUpdateAsync(challenge, UpdateType.ChallengeDeleted,
                $"Challenge {challengeInfo} has been removed.");
        }

        public async Task<ChallengeCompletion> RecordCompletionAsync(Guid challengeId, Guid teamId, string notes)
        {
            var challenge = await GetChallengeByIdAsync(challengeId);
            if (challenge == null)
            {
                throw new InvalidOperationException($"Challenge with ID {challengeId} not found.");
            }

            // Check if team has already completed this challenge
            if (await _teamService.HasCompletedChallengeAsync(teamId, challengeId))
            {
                throw new InvalidOperationException("Team has already completed this challenge.");
            }

            // Check if unique challenge is still available
            if (challenge.IsUnique && !await IsUniqueCompletionAvailableAsync(challengeId))
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
            await _teamService.AwardPointsAsync(teamId, challenge.Points, $"completing challenge '{challenge.Name}'");

            // Create and broadcast update
            await _updateService.CreateChallengeCompletionUpdateAsync(completion,
                $"Team has completed challenge '{challenge.Name}' and earned {challenge.Points} points!");

            return completion;
        }

        public async Task<bool> IsUniqueCompletionAvailableAsync(Guid challengeId)
        {
            var challenge = await GetChallengeByIdAsync(challengeId);
            return challenge != null && (!challenge.IsUnique || !challenge.Completions.Any());
        }

        public async Task<IEnumerable<ChallengeCompletion>> GetCompletionsForChallengeAsync(Guid challengeId)
        {
            return await _context.ChallengeCompletions
                .Include(cc => cc.Team)
                .Where(cc => cc.ChallengeId == challengeId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetTeamsCompletedChallengeAsync(Guid challengeId)
        {
            return await _context.ChallengeCompletions
                .Include(cc => cc.Team)
                .Where(cc => cc.ChallengeId == challengeId)
                .Select(cc => cc.Team)
                .ToListAsync();
        }

        public async Task<int> GetTotalCompletionsAsync(Guid challengeId)
        {
            return await _context.ChallengeCompletions
                .CountAsync(cc => cc.ChallengeId == challengeId);
        }

        public async Task RemoveCompletionAsync(Guid challengeId, Guid teamId)
        {
            var completion = await _context.ChallengeCompletions
                .FirstOrDefaultAsync(cc => cc.ChallengeId == challengeId && cc.TeamId == teamId);

            if (completion == null)
            {
                throw new InvalidOperationException("Challenge completion not found.");
            }

            _context.ChallengeCompletions.Remove(completion);
            await _context.SaveChangesAsync();

            // Deduct points from the team
            await _teamService.AwardPointsAsync(teamId, -completion.PointsAwarded, 
                "challenge completion removed");
        }

        public async Task<IDictionary<Challenge, int>> GetChallengeCompletionStatsAsync()
        {
            var challenges = await _context.Challenges
                .Include(c => c.Completions)
                .ToListAsync();

            return challenges.ToDictionary(
                c => c,
                c => c.Completions.Count
            );
        }

        public async Task<IEnumerable<Challenge>> GetMostPopularChallengesAsync(int count = 5)
        {
            return await _context.Challenges
                .Include(c => c.Completions)
                .OrderByDescending(c => c.Completions.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Challenge>> GetAvailableUniqueChallengesAsync()
        {
            return await _context.Challenges
                .Include(c => c.Completions)
                .Where(c => c.IsUnique && !c.Completions.Any())
                .ToListAsync();
        }
    }
}
