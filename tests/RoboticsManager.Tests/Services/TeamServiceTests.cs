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
    public class TeamServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IUpdateService> _updateServiceMock;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _updateServiceMock = new Mock<IUpdateService>();
            _teamService = new TeamService(_context, _updateServiceMock.Object);
        }

        [Fact]
        public async Task CreateTeam_ValidTeam_CreatesSuccessfully()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team",
                TeamNo = "T123",
                School = "Test School",
                Color = "#FF0000"
            };

            // Act
            var result = await _teamService.CreateTeamAsync(team);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(team.Name, result.Name);
            Assert.Equal(team.TeamNo, result.TeamNo);
            Assert.Equal(0, result.TotalPoints);
            _updateServiceMock.Verify(x => x.CreateTeamUpdateAsync(
                It.IsAny<Team>(),
                It.Is<UpdateType>(t => t == UpdateType.TeamCreated),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task CreateTeam_DuplicateTeamNo_ThrowsException()
        {
            // Arrange
            var team1 = new Team
            {
                Name = "Team 1",
                TeamNo = "T123",
                School = "School 1",
                Color = "#FF0000"
            };

            var team2 = new Team
            {
                Name = "Team 2",
                TeamNo = "T123", // Same team number
                School = "School 2",
                Color = "#00FF00"
            };

            await _teamService.CreateTeamAsync(team1);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _teamService.CreateTeamAsync(team2)
            );
        }

        [Fact]
        public async Task GetLeaderboard_ReturnsTeamsOrderedByPoints()
        {
            // Arrange
            var teams = new[]
            {
                new Team { Name = "Team 1", TeamNo = "T1", School = "School 1", Color = "#FF0000", TotalPoints = 100 },
                new Team { Name = "Team 2", TeamNo = "T2", School = "School 2", Color = "#00FF00", TotalPoints = 300 },
                new Team { Name = "Team 3", TeamNo = "T3", School = "School 3", Color = "#0000FF", TotalPoints = 200 }
            };

            foreach (var team in teams)
            {
                _context.Teams.Add(team);
            }
            await _context.SaveChangesAsync();

            // Act
            var leaderboard = (await _teamService.GetLeaderboardAsync()).ToList();

            // Assert
            Assert.Equal(3, leaderboard.Count);
            Assert.Equal("Team 2", leaderboard[0].Name); // 300 points
            Assert.Equal("Team 3", leaderboard[1].Name); // 200 points
            Assert.Equal("Team 1", leaderboard[2].Name); // 100 points
        }

        [Fact]
        public async Task AwardPoints_ValidTeam_UpdatesPoints()
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
            await _teamService.CreateTeamAsync(team);

            // Act
            var result = await _teamService.AwardPointsAsync(team.Id, 50, "Test award");

            // Assert
            Assert.Equal(150, result.TotalPoints);
            _updateServiceMock.Verify(x => x.CreateTeamUpdateAsync(
                It.IsAny<Team>(),
                It.Is<UpdateType>(t => t == UpdateType.PointsAwarded),
                It.IsAny<string>()
            ), Times.Once);
            _updateServiceMock.Verify(x => x.BroadcastLeaderboardUpdateAsync(
                It.IsAny<IEnumerable<Team>>()
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteTeam_ExistingTeam_DeletesSuccessfully()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team",
                TeamNo = "T123",
                School = "Test School",
                Color = "#FF0000"
            };
            await _teamService.CreateTeamAsync(team);

            // Act
            await _teamService.DeleteTeamAsync(team.Id);

            // Assert
            var deletedTeam = await _context.Teams.FindAsync(team.Id);
            Assert.Null(deletedTeam);
            _updateServiceMock.Verify(x => x.CreateTeamUpdateAsync(
                It.IsAny<Team>(),
                It.Is<UpdateType>(t => t == UpdateType.TeamDeleted),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task UpdateTeam_ValidTeam_UpdatesSuccessfully()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team",
                TeamNo = "T123",
                School = "Test School",
                Color = "#FF0000"
            };
            await _teamService.CreateTeamAsync(team);

            // Update team properties
            team.Name = "Updated Team";
            team.School = "Updated School";

            // Act
            var result = await _teamService.UpdateTeamAsync(team);

            // Assert
            Assert.Equal("Updated Team", result.Name);
            Assert.Equal("Updated School", result.School);
            _updateServiceMock.Verify(x => x.CreateTeamUpdateAsync(
                It.IsAny<Team>(),
                It.Is<UpdateType>(t => t == UpdateType.TeamUpdated),
                It.IsAny<string>()
            ), Times.Once);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
