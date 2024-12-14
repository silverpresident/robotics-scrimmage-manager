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
    public class AnnouncementServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IUpdateService> _updateServiceMock;
        private readonly AnnouncementService _announcementService;

        public AnnouncementServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _updateServiceMock = new Mock<IUpdateService>();
            _announcementService = new AnnouncementService(_context, _updateServiceMock.Object);
        }

        [Fact]
        public async Task CreateAnnouncement_ValidAnnouncement_CreatesAndRendersMarkdown()
        {
            // Arrange
            var announcement = new Announcement
            {
                Body = "# Test Announcement\n\nThis is a **bold** test.",
                Priority = AnnouncementPriority.Info,
                IsVisible = true
            };

            // Act
            var result = await _announcementService.CreateAnnouncementAsync(announcement);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("<h1>", result.RenderedBody);
            Assert.Contains("<strong>bold</strong>", result.RenderedBody);
            _updateServiceMock.Verify(x => x.CreateAnnouncementUpdateAsync(
                It.IsAny<Announcement>(),
                It.Is<UpdateType>(t => t == UpdateType.AnnouncementCreated),
                It.IsAny<string>()
            ), Times.Once);
            _updateServiceMock.Verify(x => x.BroadcastAnnouncementAsync(
                It.IsAny<Announcement>()
            ), Times.Once);
        }

        [Fact]
        public async Task GetVisibleAnnouncements_ReturnsOnlyVisibleAnnouncements()
        {
            // Arrange
            var announcements = new[]
            {
                new Announcement { Body = "Visible 1", IsVisible = true, Priority = AnnouncementPriority.Info },
                new Announcement { Body = "Hidden", IsVisible = false, Priority = AnnouncementPriority.Warning },
                new Announcement { Body = "Visible 2", IsVisible = true, Priority = AnnouncementPriority.Info }
            };

            foreach (var announcement in announcements)
            {
                await _announcementService.CreateAnnouncementAsync(announcement);
            }

            // Act
            var visibleAnnouncements = (await _announcementService.GetVisibleAnnouncementsAsync()).ToList();

            // Assert
            Assert.Equal(2, visibleAnnouncements.Count);
            Assert.All(visibleAnnouncements, a => Assert.True(a.IsVisible));
        }

        [Fact]
        public async Task CreateChallengeCompletionAnnouncement_CreatesFormattedAnnouncement()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team",
                TeamNo = "T123",
                School = "Test School",
                Color = "#FF0000"
            };
            _context.Teams.Add(team);

            var challenge = new Challenge
            {
                Name = "Test Challenge",
                Description = "Test Description",
                Points = 100,
                IsUnique = true
            };
            _context.Challenges.Add(challenge);

            var completion = new ChallengeCompletion
            {
                TeamId = team.Id,
                ChallengeId = challenge.Id,
                PointsAwarded = 100
            };
            _context.ChallengeCompletions.Add(completion);
            await _context.SaveChangesAsync();

            // Act
            await _announcementService.CreateChallengeCompletionAnnouncementAsync(completion);

            // Assert
            var announcement = await _context.Announcements.FirstOrDefaultAsync();
            Assert.NotNull(announcement);
            Assert.Contains("Test Team", announcement.Body);
            Assert.Contains("Test Challenge", announcement.Body);
            Assert.Contains("100 points", announcement.Body);
            Assert.Contains("unique challenge", announcement.Body.ToLower());
            Assert.Equal(AnnouncementPriority.Info, announcement.Priority);
        }

        [Fact]
        public async Task GetAnnouncementsByPriority_ReturnsMatchingAnnouncements()
        {
            // Arrange
            var announcements = new[]
            {
                new Announcement { Body = "Info", Priority = AnnouncementPriority.Info, IsVisible = true },
                new Announcement { Body = "Warning", Priority = AnnouncementPriority.Warning, IsVisible = true },
                new Announcement { Body = "Info 2", Priority = AnnouncementPriority.Info, IsVisible = true }
            };

            foreach (var announcement in announcements)
            {
                await _announcementService.CreateAnnouncementAsync(announcement);
            }

            // Act
            var infoAnnouncements = (await _announcementService.GetAnnouncementsByPriorityAsync(AnnouncementPriority.Info)).ToList();

            // Assert
            Assert.Equal(2, infoAnnouncements.Count);
            Assert.All(infoAnnouncements, a => Assert.Equal(AnnouncementPriority.Info, a.Priority));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
