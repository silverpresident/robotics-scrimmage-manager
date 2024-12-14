using System;
using System.ComponentModel.DataAnnotations;

namespace RoboticsManager.Lib.Models
{
    public enum UpdateType
    {
        TeamCreated,
        TeamUpdated,
        TeamDeleted,
        ChallengeCreated,
        ChallengeUpdated,
        ChallengeDeleted,
        ChallengeCompleted,
        ChallengeCompletion,
        AnnouncementCreated,
        AnnouncementUpdated,
        AnnouncementDeleted,
        PointsAwarded
    }

    public class Update : BaseEntity
    {
        [Required]
        public UpdateType Type { get; set; }

        [Required]
        public string Description { get; set; }

        // Optional reference to related entities
        public Guid? TeamId { get; set; }
        public Guid? ChallengeId { get; set; }
        public Guid? AnnouncementId { get; set; }
        public Guid? ChallengeCompletionId { get; set; }

        // Additional metadata stored as JSON
        public string Metadata { get; set; }

        // For real-time updates, track if this has been broadcast
        public bool IsBroadcast { get; set; }

        // Navigation properties
        public virtual Team Team { get; set; }
        public virtual Challenge Challenge { get; set; }
        public virtual Announcement Announcement { get; set; }
        public virtual ChallengeCompletion ChallengeCompletion { get; set; }
    }
}
