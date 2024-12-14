using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoboticsManager.Lib.Models
{
    public class Team : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(20)]
        public required string TeamNo { get; set; }

        [Required]
        [StringLength(100)]
        public required string School { get; set; }

        [Required]
        [StringLength(7)] // #RRGGBB format
        public required string Color { get; set; }

        [Url]
        [StringLength(2048)]
        public string? LogoUrl { get; set; }

        public int TotalPoints { get; set; }

        // Navigation properties
        public virtual ICollection<ChallengeCompletion> CompletedChallenges { get; private set; }

        public Team()
        {
            CompletedChallenges = new HashSet<ChallengeCompletion>();
            TotalPoints = 0;
        }

        // Helper methods for managing the collection
        public void AddCompletion(ChallengeCompletion completion)
        {
            CompletedChallenges.Add(completion);
            TotalPoints += completion.PointsAwarded;
        }

        public void RemoveCompletion(ChallengeCompletion completion)
        {
            if (CompletedChallenges.Remove(completion))
            {
                TotalPoints -= completion.PointsAwarded;
            }
        }

        public void UpdatePoints(int pointsDelta)
        {
            TotalPoints += pointsDelta;
        }
    }
}
