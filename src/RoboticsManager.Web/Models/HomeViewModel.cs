using RoboticsManager.Lib.Models;

namespace RoboticsManager.Web.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Team> Leaderboard { get; set; } = new List<Team>();
        public IEnumerable<Announcement> Announcements { get; set; } = new List<Announcement>();
        public IEnumerable<Challenge> RecentChallenges { get; set; } = new List<Challenge>();
        public IEnumerable<Update> RecentUpdates { get; set; } = new List<Update>();

        public int TotalTeams { get; set; }
        public int TotalChallenges { get; set; }
        public int TotalCompletions { get; set; }
        public int TotalPoints { get; set; }

        public class LeaderboardStats
        {
            public string TeamName { get; set; } = string.Empty;
            public int CompletedChallenges { get; set; }
            public int TotalPoints { get; set; }
            public DateTime? LastCompletion { get; set; }
        }

        public class ChallengeStats
        {
            public string ChallengeName { get; set; } = string.Empty;
            public int CompletionCount { get; set; }
            public int TotalPoints { get; set; }
            public bool IsUnique { get; set; }
            public DateTime? FirstCompletion { get; set; }
        }

        public IEnumerable<LeaderboardStats> TopTeams { get; set; } = new List<LeaderboardStats>();
        public IEnumerable<ChallengeStats> PopularChallenges { get; set; } = new List<ChallengeStats>();

        public class EventTimeline
        {
            public DateTime Timestamp { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty; // "challenge", "announcement", "update"
            public string Priority { get; set; } = string.Empty; // For styling
            public Guid? ReferenceId { get; set; } // ID of the related entity
        }

        public IEnumerable<EventTimeline> Timeline { get; set; } = new List<EventTimeline>();

        public HomeViewModel()
        {
        }

        public HomeViewModel(
            IEnumerable<Team> leaderboard,
            IEnumerable<Announcement> announcements,
            IEnumerable<Challenge> recentChallenges,
            IEnumerable<Update> recentUpdates)
        {
            Leaderboard = leaderboard.OrderByDescending(t => t.TotalPoints);
            Announcements = announcements.Where(a => a.IsVisible).OrderByDescending(a => a.CreatedAt);
            RecentChallenges = recentChallenges.OrderByDescending(c => c.CreatedAt);
            RecentUpdates = recentUpdates.OrderByDescending(u => u.CreatedAt);

            TotalTeams = leaderboard.Count();
            TotalChallenges = recentChallenges.Count();
            TotalPoints = leaderboard.Sum(t => t.TotalPoints);
        }

        public void GenerateTimeline()
        {
            var timeline = new List<EventTimeline>();

            // Add announcements to timeline
            timeline.AddRange(Announcements.Select(a => new EventTimeline
            {
                Timestamp = a.CreatedAt,
                Description = a.Body,
                Type = "announcement",
                Priority = a.Priority.ToString().ToLower(),
                ReferenceId = a.Id
            }));

            // Add updates to timeline
            timeline.AddRange(RecentUpdates.Select(u => new EventTimeline
            {
                Timestamp = u.CreatedAt,
                Description = u.Description,
                Type = "update",
                Priority = u.Type.ToString().ToLower(),
                ReferenceId = u.Id
            }));

            // Sort timeline by timestamp descending
            Timeline = timeline.OrderByDescending(t => t.Timestamp);
        }
    }
}
