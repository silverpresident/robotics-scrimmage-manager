using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoboticsManager.Lib.Models
{
    public class Challenge : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Points { get; set; }

        public bool IsUnique { get; set; }

        // Navigation properties
        public virtual ICollection<ChallengeCompletion> Completions { get; set; }

        public Challenge()
        {
            Completions = new HashSet<ChallengeCompletion>();
        }
    }
}
