using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Hubs;
using RoboticsManager.Lib.Models;
using RoboticsManager.Lib.Services.Implementations;
using Xunit;

namespace RoboticsManager.Tests.Services
{
    public class UpdateServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IHubContext<UpdateHub, IUpdateClient>> _hubContextMock;
        private readonly Mock<IUpdateClient> _clientProxyMock;
        private readonly UpdateService _updateService;

        public UpdateServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _hubContextMock = new Mock<IHubContext<UpdateHub, IUpdateClient>>();
            _clientProxyMock = new Mock<IUpdateClient>();

            // Setup hub context mock
            _hubContextMock.Setup(x => x.Clients.All)
                .Returns(_clientProxyMock.Object);

            _updateService = new UpdateService(_context, _hubContextMock.Object);
        }

        [Fact]
        public async Task CreateUpdate_StoresUpdateAndBroadcasts()
        {
            // Arrange
            var metadata = new { key = "value" };

            // Act
            var result = await _updateService.CreateUpdateAsync(
                UpdateType.TeamCreated,
                "Test update",
                metadata
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(UpdateType.TeamCreated, result.Type);
            Assert.Equal("Test update", result.Description);
            Assert.Contains("value", result.Metadata);

            _clientProxyMock.Verify(x => x.ReceiveUpdate(
                It.Is<Update>(u => u.Type == UpdateType.TeamCreated)
            ), Times.Once);
        }

        [Fact]
        public async Task CreateTeamUpdate_IncludesTeamMetadata()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team",
                TeamNo = "T123",
                School = "Test School",
                Color = "#FF0000",
                TotalPoints = 100
            };

            // Act
            var result = await _updateService.CreateTeamUpdateAsync(
                team,
                UpdateType.TeamUpdated,
                "Team updated"
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(team.Id, result.TeamId);
            Assert.Contains(team.Name, result.Metadata);
            Assert.Contains(team.TeamNo, result.Metadata);
            Assert.Contains(team.School, result.Metadata);

            _clientProxyMock.Verify(x => x.ReceiveUpdate(
                It.Is<Update>(u => u.TeamId == team.Id)
            ), Times.Once);
            _clientProxyMock.Verify(x => x.ReceiveTeamUpdate(
                It.Is<Team>(t => t.Id == team.Id)
            ), Times.Once);
        }

        [Fact]
        public async Task CreateChallengeUpdate_IncludesChallengeMetadata()
        {
            // Arrange
            var challenge = new Challenge
            {
                Name = "Test Challenge",
                Description = "Test Description",
                Points = 100,
                IsUnique = true
            };

            // Act
            var result = await _updateService.CreateChallengeUpdateAsync(
                challenge,
                UpdateType.ChallengeCreated,
                "Challenge created"
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(challenge.Id, result.ChallengeId);
            Assert.Contains(challenge.Name, result.Metadata);
            Assert.Contains("100", result.Metadata);
            Assert.Contains("true", result.Metadata.ToLower());

            _clientProxyMock.Verify(x => x.ReceiveUpdate(
                It.Is<Update>(u => u.ChallengeId == challenge.Id)
            ), Times.Once);
            _clientProxyMock.Verify(x => x.ReceiveChallengeUpdate(
                It.Is<Challenge>(c => c.Id == challenge.Id)
            ), Times.Once);
        }

        [Fact]
        public async Task BroadcastLeaderboardUpdate_SendsLeaderboardToAllClients()
        {
            // Arrange
            var teams = new[]
            {
                new Team { Name = "Team 1", TeamNo = "T1", School = "School 1", Color = "#FF0000", TotalPoints = 100 },
                new Team { Name = "Team 2", TeamNo = "T2", School = "School 2", Color = "#00FF00", TotalPoints = 200 }
            };

            // Act
            await _updateService.BroadcastLeaderboardUpdateAsync(teams);

            // Assert
            _clientProxyMock.Verify(x => x.ReceiveLeaderboardUpdate(
                It.Is<Team[]>(t => t.Length == 2)
            ), Times.Once);
        }

        [Fact]
        public async Task CleanupOldUpdates_RemovesUpdatesOlderThanSpecifiedDays()
        {
            // Arrange
            var oldUpdate = new Update
            {
                Type = UpdateType.TeamCreated,
                Description = "Old update",
                CreatedAt = DateTime.UtcNow.AddDays(-31)
            };
            var recentUpdate = new Update
            {
                Type = UpdateType.TeamCreated,
                Description = "Recent update",
                CreatedAt = DateTime.UtcNow
            };

            _context.Updates.AddRange(oldUpdate, recentUpdate);
            await _context.SaveChangesAsync();

            // Act
            await _updateService.CleanupOldUpdatesAsync(30);

            // Assert
            var remainingUpdates = await _context.Updates.ToListAsync();
            Assert.Single(remainingUpdates);
            Assert.Equal("Recent update", remainingUpdates[0].Description);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
