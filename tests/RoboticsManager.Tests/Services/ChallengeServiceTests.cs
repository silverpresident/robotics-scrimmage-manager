using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Models;
using RoboticsManager.Lib.Services;
using RoboticsManager.Lib.Services.Implementations;
using Xunit;

namespace RoboticsManager.Tests.Services
{
    public class ChallengeServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IUpdateService> _updateServiceMock;
        private readonly Mock<ITeamService> _teamServiceMock;
        private readonly ChallengeService _challengeService;

        public ChallengeServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _updateServiceMock = new Mock<IUpdateService>();
            _teamServiceMock = new Mock<ITeamService>();
            _challengeService = new ChallengeService(_context, _updateServiceMock.Object, _teamServiceMock.Object);
        }

        [Fact]
        public async Task CreateChallenge_ValidChallenge_CreatesSuccessfully()
        {
            // Arrange
            var challenge = new Challenge
            {
                Name = "Test Challenge",
                Description = "Test challenge description",
                Points = 100,
                IsUnique = true
            };

            // Act
            var result = await _challengeService.CreateChallengeAsync(challenge);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(challenge.Name, result.Name);
            Assert.Equal(challenge.Points, result.Points);
            Assert.True(result.IsUnique);
            _updateServiceMock.Verify(x => x.CreateChallengeUpdateAsync(
                It.IsAny<Challenge>(),
                It.Is<UpdateType>(t => t == UpdateType.ChallengeCreated),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task CreateChallenge_DuplicateName_ThrowsException()
        {
            // Arrange
            var challenge1 = new Challenge
            {
                Name = "Test Challenge",
                Description = "Description 1",
                Points = 100
            };

            var challenge2 = new Challenge
            {
                Name = "Test Challenge", // Same name
                Description = "Description 2",
                Points = 200
            };

            await _challengeService.CreateChallengeAsync(challenge1);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _challengeService.CreateChallengeAsync(challenge2)
            );
        }

        [Fact]
        public async Task RecordCompletion_UniqueChallenge_FirstTeamSucceeds()
        {
            // Arrange
            var challenge = new Challenge
            {
                Name = "Unique Challenge",
                Description = "Test unique challenge description",
                Points = 100,
                IsUnique = true
            };
            await _challengeService.CreateChallengeAsync(challenge);

            var teamId = Guid.NewGuid();
            _teamServiceMock.Setup(x => x.HasCompletedChallengeAsync(teamId, challenge.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _challengeService.RecordCompletionAsync(challenge.Id, teamId, "First completion");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.PointsAwarded);
            _teamServiceMock.Verify(x => x.AwardPointsAsync(
                teamId,
                100,
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task GetAvailableUniqueChallenges_ReturnsOnlyUncompletedUniqueChallenges()
        {
            // Arrange
            var challenges = new[]
            {
                new Challenge { Name = "Unique 1", Description = "Description 1", Points = 100, IsUnique = true },
                new Challenge { Name = "Unique 2", Description = "Description 2", Points = 200, IsUnique = true },
                new Challenge { Name = "Regular", Description = "Description 3", Points = 50, IsUnique = false }
            };

            foreach (var challenge in challenges)
            {
                await _challengeService.CreateChallengeAsync(challenge);
            }

            // Complete one unique challenge
            var teamId = Guid.NewGuid();
            _teamServiceMock.Setup(x => x.HasCompletedChallengeAsync(teamId, It.IsAny<Guid>()))
                .ReturnsAsync(false);
            await _challengeService.RecordCompletionAsync(challenges[0].Id, teamId, "Test completion");

            // Act
            var availableChallenges = (await _challengeService.GetAvailableUniqueChallengesAsync()).ToList();

            // Assert
            Assert.Single(availableChallenges);
            Assert.Equal("Unique 2", availableChallenges[0].Name);
        }

        [Fact]
        public async Task GetChallengeCompletionStats_ReturnsCorrectCounts()
        {
            // Arrange
            var challenge = new Challenge
            {
                Name = "Test Challenge",
                Description = "Test challenge description",
                Points = 100,
                IsUnique = false
            };
            await _challengeService.CreateChallengeAsync(challenge);

            // Record multiple completions
            var teamIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            foreach (var teamId in teamIds)
            {
                _teamServiceMock.Setup(x => x.HasCompletedChallengeAsync(teamId, challenge.Id))
                    .ReturnsAsync(false);
                await _challengeService.RecordCompletionAsync(challenge.Id, teamId, "Test completion");
            }

            // Act
            var stats = await _challengeService.GetChallengeCompletionStatsAsync();

            // Assert
            Assert.Single(stats);
            Assert.Equal(3, stats[challenge]);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
