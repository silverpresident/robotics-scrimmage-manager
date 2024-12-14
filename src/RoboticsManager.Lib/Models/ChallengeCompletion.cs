using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoboticsManager.Lib.Models
{
    public class ChallengeCompletion : BaseEntity
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid ChallengeId { get; set; }

        [Required]
        public int PointsAwarded { get; set; }

        public string Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(TeamId))]
        public virtual Team Team { get; set; }

        [ForeignKey(nameof(ChallengeId))]
        public virtual Challenge Challenge { get; set; }
    }
}
