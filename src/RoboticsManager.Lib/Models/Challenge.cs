using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoboticsManager.Lib.Models
{
    public class Challenge : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Points { get; set; }

        public bool IsUnique { get; set; }

        // Navigation properties
        public virtual ICollection<ChallengeCompletion> Completions { get; private set; }

        public Challenge()
        {
            Completions = new HashSet<ChallengeCompletion>();
            Points = 0;
            IsUnique = false;
        }

        // Helper methods for managing completions
        public void AddCompletion(ChallengeCompletion completion)
        {
            if (IsUnique && Completions.Any())
            {
                throw new InvalidOperationException("Cannot add multiple completions to a unique challenge.");
            }
            Completions.Add(completion);
        }

        public void RemoveCompletion(ChallengeCompletion completion)
        {
            Completions.Remove(completion);
        }

        public bool HasBeenCompleted()
        {
            return Completions.Any();
        }

        public bool CanBeCompletedBy(Team team)
        {
            if (!IsUnique)
            {
                return !Completions.Any(c => c.TeamId == team.Id);
            }
            return !HasBeenCompleted();
        }
    }
}
