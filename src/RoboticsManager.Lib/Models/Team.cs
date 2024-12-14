using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoboticsManager.Lib.Models
{
    public class Team : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string TeamNo { get; set; }

        [Required]
        [StringLength(100)]
        public string School { get; set; }

        [Required]
        [StringLength(7)] // #RRGGBB format
        public string Color { get; set; }

        [Url]
        [StringLength(2048)]
        public string LogoUrl { get; set; }

        public int TotalPoints { get; set; }

        // Navigation properties
        public virtual ICollection<ChallengeCompletion> CompletedChallenges { get; set; }

        public Team()
        {
            CompletedChallenges = new HashSet<ChallengeCompletion>();
            TotalPoints = 0;
        }
    }
}
